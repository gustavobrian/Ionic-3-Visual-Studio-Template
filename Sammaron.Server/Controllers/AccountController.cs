using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sammaron.Authentication.JwtBearer;
using Sammaron.Authentication.JwtBearer.Extensions;
using Sammaron.Core.Data;
using Sammaron.Core.Interfaces;
using Sammaron.Core.Models;
using Sammaron.Server.Results;
using Sammaron.Server.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sammaron.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/account")]
    [EnableCors("AllowAll")]
    public class AccountController : Controller
    {
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
            IMapper mapper, ApiContext context, ISmsSender smsSender, IHostingEnvironment environment)
        {
            Environment = environment;
            UserManager = userManager;
            SignInManager = signInManager;
            Mapper = mapper;
            Context = context;
            SmsSender = smsSender;
        }

        public ApiContext Context
        { get; }

        public IHostingEnvironment Environment { get; }
        public string ImagesPath => $"{Environment.WebRootPath}\\images";
        public IMapper Mapper { get; }
        public SignInManager<User> SignInManager { get; }
        public ISmsSender SmsSender { get; }
        public UserManager<User> UserManager { get; protected set; }

        // POST api/Account/ChangePassword
        [HttpPost, Route("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await UserManager.GetUserAsync(User);
            var result = await UserManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors.ToDictionary(e => e.Code, e => e.Description));

            return await SignInResultAsync(user);
        }

        // POST api/Account/Token
        [AllowAnonymous, HttpPost, Route("token")]
        public async Task<IActionResult> Token(GrantType grant_type, [FromBody] LoginModel model)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            switch (grant_type)
            {
                case GrantType.password:
                    return await PasswordLogin(model);

                case GrantType.refresh_token:
                    return SignIn(model.UserIdentifier);

                case GrantType.totp:
                    return await TOTPLogin(model);

                default:
                    throw new ArgumentOutOfRangeException(nameof(grant_type));
            }
        }

        // POST api/Account/Logout
        [HttpPost, Route("logout")]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties(new Dictionary<string, string>
            {
                { BearerConstants.UserIdKey,  User.FindFirstValue(ClaimTypes.NameIdentifier)}
            }), JwtBearerDefaults.AuthenticationScheme);
        }

        [AllowAnonymous, HttpPost, Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = Mapper.Map<RegisterModel, User>(model);
            if (!string.IsNullOrEmpty(model.ProfileImage))
            {
                model.CreateImage(ImagesPath, out var profileImageUrl);
                user.ProfileImageUrl = $"{Request.Scheme}://{Request.Host}/{profileImageUrl}";
            }

            var result = await CreateUserAsync(user, model.Password);
            if (result.Succeeded)
                return Ok();
            return BadRequest(result.Errors.ToDictionary(e => e.Code, e => e.Description));
        }

        [HttpPost, Route("setPassword")]
        public async Task<IActionResult> SetPassword([FromBody] PasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await UserManager.GetUserAsync(User);
            var result = await UserManager.AddPasswordAsync(user, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors.ToDictionary(e => e.Code, e => e.Description));

            return await SignInResultAsync(user);
        }

        [HttpPost, Route("updateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UserManager.GetUserAsync(User);
            if (user == null)
                return BadRequest();

            if (!string.IsNullOrEmpty(model.ProfileImage))
            {
                model.CreateImage(ImagesPath, out var profileImageUrl);
                user.ProfileImageUrl = $"{Request.Scheme}://{Request.Host}/{profileImageUrl}";
            }

            Mapper.Map(model, user);
            var result = await UserManager.UpdateAsync(user);

            if (result.Succeeded)
                return await SignInResultAsync(user);
            return BadRequest(result.Errors.ToDictionary(e => e.Code, e => e.Description));
        }

        protected async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return !string.IsNullOrEmpty(password)
                ? await UserManager.CreateAsync(user, password)
                : await UserManager.CreateAsync(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }

            base.Dispose(disposing);
        }

        [NonAction]
        protected async Task<IActionResult> PasswordLogin(LoginModel model)
        {
            var user = await UserManager.FindByNameAsync(model.UserIdentifier);
            if (user == null)
                return BadRequest();

            var isValidPassword = await UserManager.CheckPasswordAsync(user, model.Password);
            if (!isValidPassword)
                return BadRequest();

            return await SignInResultAsync(user);
        }

        [NonAction]
        protected virtual RefreshTokenSignInResult SignIn(string refreshToken)
        {
            return new RefreshTokenSignInResult(refreshToken);
        }

        [NonAction]
        protected async Task<IActionResult> SignInResultAsync(User user)
        {
            var principal = await SignInManager.ClaimsFactory.CreateClaimsAsync(user);
            var userData = new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Headline,
                user.Summary,
                user.Email,
                user.ProfileImageUrl,
                user.PhoneNumber
            }.ToJsonString();
            return SignIn(principal, new AuthenticationProperties(new Dictionary<string, string>
            {
                { BearerConstants.UserDataKey, userData }
            }), JwtBearerDefaults.AuthenticationScheme);
        }

        [NonAction]
        protected async Task<IActionResult> TOTPLogin(LoginModel model)
        {
            var phoneNumber = Regex.Match(model.UserIdentifier, @"(.{9})\s*$").Value;
            var user = Context.Users.FirstOrDefault(e => e.PhoneNumber.EndsWith(phoneNumber));
            if (user == null)
                return BadRequest();
            var purpose = $"TOTP:{model.UserIdentifier}";
            if (string.IsNullOrEmpty(model.Password))
            {
                model.Password = await UserManager.GenerateUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, purpose);
                if (Environment.IsDevelopment())
                {
                    return Ok(new
                    {
                        model.Password
                    });
                }

                await SmsSender.SendSmsAsync(model.UserIdentifier, $"Your phone number verification code {model.Password}");
                return Ok(new
                {
                    model.Password
                });
            }

            var isValidCode = await UserManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, purpose, model.Password);
            if (!isValidCode)
                return BadRequest();

            return await SignInResultAsync(user);
        }
    }
}