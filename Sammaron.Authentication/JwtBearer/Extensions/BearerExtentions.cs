using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sammaron.Authentication.JwtBearer.Exceptions;
using Sammaron.Core.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sammaron.Authentication.JwtBearer.Extensions
{
    public static class UserClaimExtentions
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static async Task<ClaimsPrincipal> CreateClaimsAsync(this IUserClaimsPrincipalFactory<User> factory, User user)
        {
            if (!(factory is UserClaimsPrincipalFactory<User> principalFactory))
                throw new ArgumentNullException(nameof(factory));
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme, ClaimTypes.NameIdentifier, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));

            if (!string.IsNullOrEmpty(user.FirstName))
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));

            if (!string.IsNullOrEmpty(user.LastName))
                identity.AddClaim(new Claim(ClaimTypes.Surname, user.FirstName));

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                identity.AddClaim(new Claim(BearerConstants.ProfileImage, user.ProfileImageUrl));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                identity.AddClaim(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

            if (principalFactory.UserManager.SupportsUserClaim)
                identity.AddClaims(await principalFactory.UserManager.GetClaimsAsync(user));

            if (principalFactory.UserManager.SupportsUserSecurityStamp)
                identity.AddClaim(new Claim(BearerConstants.SecurityStamp, user.SecurityStamp));

            if (principalFactory.UserManager.SupportsUserRole)
            {
                var roles = await principalFactory.UserManager.GetRolesAsync(user);
                identity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            return new ClaimsPrincipal(identity);
        }

        public static byte[] GetHash(this string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(this string inputString)
        {
            var bytes = inputString.GetHash();
            return bytes.Aggregate(string.Empty, (current, b) => current + b.ToString("x2"));
        }

        /// <summary>
        /// Adds and configures the identity system for the specified User and Role types.
        /// </summary>
        /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
        /// <typeparam name="TRole">The type representing a Role in the system.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <param name="setupAction">An action to configure the <see cref="T:Microsoft.AspNetCore.Identity.IdentityOptions" />.</param>
        /// <returns>An <see cref="T:Microsoft.AspNetCore.Identity.IdentityBuilder" /> for creating and configuring the identity system.</returns>
        public static IdentityBuilder AddIdentityBase<TUser, TRole>(this IServiceCollection services, Action<IdentityOptions> setupAction) where TUser : class where TRole : class
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();
            services.TryAddScoped<UserManager<TUser>, AspNetUserManager<TUser>>();
            services.TryAddScoped<SignInManager<TUser>, SignInManager<TUser>>();
            services.TryAddScoped<RoleManager<TRole>, AspNetRoleManager<TRole>>();
            if (setupAction != null)
                services.Configure(setupAction);
            return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
        }

        public static Task SignInAsync(this HttpContext context, string scheme, string refreshToken)
        {
            return context.RequestServices.GetRequiredService<IBearerAuthenticationService>().SignInAsync(context, scheme, refreshToken);
        }

        public static (string error, string description) CreateErrorDescription(this Exception authFailure)
        {
            var description = string.Empty;
            var error = string.Empty;
            switch (authFailure)
            {
                case SecurityTokenInvalidAudienceException _:
                    error = "invalid_token";
                    description = "The audience is invalid";
                    break;

                case SecurityTokenInvalidIssuerException _:
                    error = "invalid_token";
                    description = "The issuer is invalid";
                    break;

                case SecurityTokenNoExpirationException _:
                    error = "invalid_token";
                    description = "The token has no expiration";
                    break;

                case SecurityTokenInvalidLifetimeException _:
                    error = "invalid_token";
                    description = "The token lifetime is invalid";
                    break;

                case SecurityTokenNotYetValidException _:
                    error = "invalid_token";
                    description = "The token is not valid yet";
                    break;

                case SecurityTokenExpiredException _:
                    error = "invalid_token";
                    description = "The token is expired";
                    break;

                case SecurityTokenSignatureKeyNotFoundException _:
                    error = "invalid_token";
                    description = "The signature key was not found";
                    break;

                case SecurityTokenInvalidSignatureException _:
                    error = "invalid_token";
                    description = "The signature is invalid";
                    break;

                case SecurityTokenInvalidStampException _:
                    error = "invalid_token";
                    description = "The access token is no longer valid";
                    break;

                case SecurityTokenException _:
                    error = "missing_token";
                    description = "The authorization header is required.";
                    break;
            }

            return (error: error, description: description);
        }

        public static string ToJsonString(this object data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public static async Task WriteJsonAsync(this HttpResponse response, object o)
        {
            await response.WriteJsonAsync(JsonConvert.SerializeObject(o, settings));
        }

        public static async Task WriteJsonAsync(this HttpResponse response, string json)
        {
            response.ContentType = "application/json";
            await response.WriteAsync(json);
        }
    }
}