using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Sammaron.Authentication.JwtBearer.Extensions;

namespace Sammaron.Server.Results
{
    public class RefreshTokenSignInResult : ActionResult
    {
        public string RefreshToken { get; }
        public string AuthenticationScheme { get; }

        public RefreshTokenSignInResult(string refreshToken) : this(refreshToken, JwtBearerDefaults.AuthenticationScheme)
        {
        }

        public RefreshTokenSignInResult(string refreshToken, string authenticationScheme)
        {
            RefreshToken = refreshToken;
            AuthenticationScheme = authenticationScheme;
        }

        /// <inheritdoc />
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (RefreshToken != null)
                await context.HttpContext.SignInAsync(AuthenticationScheme, RefreshToken);
        }
    }
}