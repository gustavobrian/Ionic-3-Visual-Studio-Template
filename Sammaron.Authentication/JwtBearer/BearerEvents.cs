using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Sammaron.Authentication.JwtBearer.Exceptions;
using Sammaron.Authentication.JwtBearer.Extensions;
using Sammaron.Core.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sammaron.Authentication.JwtBearer
{
    public class BearerEvents
    {
        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        public Func<AuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } = context =>
        {
            context.Fail(context.Exception);
            return Task.CompletedTask;
        };

        /// <summary>Invoked when a protocol message is first received.</summary>
        public Func<MessageReceivedContext, Task> OnMessageReceived { get; set; } = context =>
        {
            string authorization = context.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorization))
            {
                context.Fail(new SecurityTokenException());
                return Task.CompletedTask;
            }

            var token = string.Empty;
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(token))
            {
                context.NoResult();
                return Task.CompletedTask;
            }
            context.Token = token;
            return Task.CompletedTask;
        };

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public Func<TokenValidatedContext, Task> OnTokenValidated { get; set; } = async context =>
        {
            var manager = context.HttpContext.RequestServices.GetService<UserManager<User>>();
            var user = await manager.GetUserAsync(context.Principal);
            var stamp = context.Principal.FindFirstValue(BearerConstants.SecurityStamp);
            if (user.SecurityStamp != stamp)
            {
                context.Fail(new SecurityTokenInvalidStampException());
            }
        };

        /// <summary>
        /// Invoked before a challenge is sent back to the caller.
        /// </summary>
        public Func<BearerChallengeContext, Task> OnChallenge { get; set; } = context =>
        {
            if (context.Options.IncludeErrorDetails && context.AuthenticateFailure != null)
            {
                var errorDescription = context.AuthenticateFailure.CreateErrorDescription();
                context.Error = errorDescription.error;
                context.ErrorDescription = errorDescription.description;
            }

            context.Response.StatusCode = 401;

            if (string.IsNullOrEmpty(context.Error) &&
                string.IsNullOrEmpty(context.ErrorDescription) &&
                string.IsNullOrEmpty(context.ErrorUri))
            {
                context.Response.Headers.Add(HeaderNames.WWWAuthenticate, context.Options.Challenge);
            }
            else
            {
                var error = context.Options.Challenge;
                if (!string.IsNullOrEmpty(context.Error))
                {
                    error += $" error=\"{context.Error}\"";
                }
                if (!string.IsNullOrEmpty(context.ErrorDescription))
                {
                    error += $" error_description=\"{context.ErrorDescription}\"";
                }
                if (!string.IsNullOrEmpty(context.ErrorUri))
                {
                    error += $" error_uri=\"{context.ErrorUri}\"";
                }
                context.Response.Headers.Add(HeaderNames.WWWAuthenticate, error);
            }

            return Task.CompletedTask;
        };

        public virtual Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            return OnAuthenticationFailed(context);
        }

        public virtual Task MessageReceived(MessageReceivedContext context)
        {
            return OnMessageReceived(context);
        }

        public virtual Task TokenValidated(TokenValidatedContext context)
        {
            return OnTokenValidated(context);
        }

        public virtual Task Challenge(BearerChallengeContext context)
        {
            return OnChallenge(context);
        }
    }
}