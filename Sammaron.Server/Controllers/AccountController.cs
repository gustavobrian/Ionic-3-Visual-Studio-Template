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
using Sammaron.Server.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sammaron.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]/[action]")]
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

        public ISmsSender SmsSender { get; }
        public IHostingEnvironment Environment { get; }

        public string ImagesPath => $"{Environment.WebRootPath}\\images";

        public IMapper Mapper { get; }
        public SignInManager<User> SignInManager { get; }
        public UserManager<User> UserManager { get; private set; }

        // POST api/Account/ChangePassword
        [HttpPost]
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

        // POST api/Account/Login
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user == null)
                return BadRequest();

            var isValidPassword = await UserManager.CheckPasswordAsync(user, model.Password);
            if (!isValidPassword)
                return BadRequest();

            return await SignInResultAsync(user);
        }

        // POST api/Account/Login
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PasswordLess([FromBody] PasswordLessModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = Context.Users.FirstOrDefault(e => e.PhoneNumber.Contains(model.PhoneNumber));
            if (user == null)
                return BadRequest();

            if (string.IsNullOrEmpty(model.Token))
            {
                model.Token = await UserManager.GenerateUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, $"TOTP:{model.PhoneNumber}");
                if (Environment.IsDevelopment())
                {
                    return Ok(new
                    {
                        model.Token
                    });
                }

                await SmsSender.SendSmsAsync(model.PhoneNumber, $"Your phone number verification code {model.Token}");
                return Ok(new
                {
                    model.Token
                });
            }

            var isValidCode = await UserManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, $"TOTP:{model.PhoneNumber}", model.Token);

            if (!isValidCode)
                return BadRequest();

            return await SignInResultAsync(user);
        }

        // POST api/Account/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties(new Dictionary<string, string>
            {
                { BearerConstants.UserIdKey,  User.FindFirstValue(ClaimTypes.NameIdentifier)}
            }), JwtBearerDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [HttpPost]
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

        [HttpPost]
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

        [HttpPost]
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }

            base.Dispose(disposing);
        }

        private async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return !string.IsNullOrEmpty(password)
                ? await UserManager.CreateAsync(user, password)
                : await UserManager.CreateAsync(user);
        }

        private async Task<IActionResult> SignInResultAsync(User user)
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
                {
                    BearerConstants.UserDataKey, userData
                }
            }), JwtBearerDefaults.AuthenticationScheme);
        }
    }
}