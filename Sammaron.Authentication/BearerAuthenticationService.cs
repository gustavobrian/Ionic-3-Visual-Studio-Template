using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Sammaron.Authentication
{
    public class BearerAuthenticationService : IBearerAuthenticationService
    {
        /// <summary>Constructor.</summary>
        /// <param name="schemes">The <see cref="T:Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider" />.</param>
        /// <param name="handlers">The <see cref="T:Microsoft.AspNetCore.Authentication.IAuthenticationRequestHandler" />.</param>
        /// <param name="transform">The The <see cref="T:Microsoft.AspNetCore.Authentication.IClaimsTransformation" />.</param>
        public BearerAuthenticationService(IAuthenticationSchemeProvider schemes, IAuthenticationHandlerProvider handlers, IClaimsTransformation transform)
        {
            Schemes = schemes;
            Handlers = handlers;
            Transform = transform;
        }

        /// <summary>Used to lookup AuthenticationSchemes.</summary>
        public IAuthenticationSchemeProvider Schemes { get; }

        /// <summary>Used to resolve IAuthenticationHandler instances.</summary>
        public IAuthenticationHandlerProvider Handlers { get; }

        /// <summary>Used for claims transformation.</summary>
        public IClaimsTransformation Transform { get; }

        /// <summary>Authenticate for the specified authentication scheme.</summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Http.HttpContext" />.</param>
        /// <param name="scheme">The name of the authentication scheme.</param>
        /// <returns>The result.</returns>
        public virtual async Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
        {
            if (scheme == null)
            {
                var authenticateSchemeAsync = await Schemes.GetDefaultAuthenticateSchemeAsync();
                scheme = authenticateSchemeAsync?.Name;
                if (scheme == null)
                    throw new InvalidOperationException("No authenticationScheme was specified, and there was no DefaultAuthenticateScheme found.");
            }
            var handlerAsync = await Handlers.GetHandlerAsync(context, scheme);
            if (handlerAsync == null)
                throw new InvalidOperationException(string.Format("No authentication handler is configured to authenticate for the scheme: {0}", scheme));
            var result = await handlerAsync.AuthenticateAsync();
            if (result != null && result.Succeeded)
                return AuthenticateResult.Success(new AuthenticationTicket(await (this).Transform.TransformAsync(result.Principal), result.Properties, result.Ticket.AuthenticationScheme));
            return result;
        }

        /// <summary>Challenge the specified authentication scheme.</summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Http.HttpContext" />.</param>
        /// <param name="scheme">The name of the authentication scheme.</param>
        /// <param name="properties">The <see cref="T:Microsoft.AspNetCore.Authentication.AuthenticationProperties" />.</param>
        /// <returns>A task.</returns>
        public virtual async Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            if (scheme == null)
            {
                var challengeSchemeAsync = await Schemes.GetDefaultChallengeSchemeAsync();
                scheme = challengeSchemeAsync?.Name;
                if (scheme == null)
                    throw new InvalidOperationException("No authenticationScheme was specified, and there was no DefaultChallengeScheme found.");
            }
            var handlerAsync = await Handlers.GetHandlerAsync(context, scheme);
            if (handlerAsync == null)
                throw new InvalidOperationException(string.Format("No authentication handler is configured to handle the scheme: {0}", scheme));
            var properties1 = properties;
            await handlerAsync.ChallengeAsync(properties1);
        }

        /// <summary>Forbid the specified authentication scheme.</summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Http.HttpContext" />.</param>
        /// <param name="scheme">The name of the authentication scheme.</param>
        /// <param name="properties">The <see cref="T:Microsoft.AspNetCore.Authentication.AuthenticationProperties" />.</param>
        /// <returns>A task.</returns>
        public virtual async Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            if (scheme == null)
            {
                var challengeSchemeAsync = await Schemes.GetDefaultChallengeSchemeAsync();
                scheme = challengeSchemeAsync?.Name;
                if (scheme == null)
                    throw new InvalidOperationException("No authenticationScheme was specified, and there was no DefaultChallengeScheme found.");
            }
            var handlerAsync = await Handlers.GetHandlerAsync(context, scheme);
            if (handlerAsync == null)
                throw new InvalidOperationException(string.Format("No authentication handler is configured to handle the scheme: {0}", scheme));
            var properties1 = properties;
            await handlerAsync.ForbidAsync(properties1);
        }

        /// <summary>
        /// Sign a principal in for the specified authentication scheme.
        /// </summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Http.HttpContext" />.</param>
        /// <param name="scheme">The name of the authentication scheme.</param>
        /// <param name="principal">The <see cref="T:System.Security.Claims.ClaimsPrincipal" /> to sign in.</param>
        /// <param name="properties">The <see cref="T:Microsoft.AspNetCore.Authentication.AuthenticationProperties" />.</param>
        /// <returns>A task.</returns>
        public virtual async Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            if (scheme == null)
            {
                var signInSchemeAsync = await Schemes.GetDefaultSignInSchemeAsync();
                scheme = signInSchemeAsync?.Name;
                if (scheme == null)
                    throw new InvalidOperationException("No authenticationScheme was specified, and there was no DefaultSignInScheme found.");
            }
            if (!(await Handlers.GetHandlerAsync(context, scheme) is IAuthenticationSignInHandler handlerAsync))
                throw new InvalidOperationException(string.Format("No IAuthenticationSignInHandler is configured to handle sign in for the scheme: {0}", scheme));
            var user = principal;
            var properties1 = properties;
            await handlerAsync.SignInAsync(user, properties1);
        }

        public async Task SignInAsync(HttpContext context, string scheme, string refreshToken)
        {
            if (scheme == null)
            {
                var signInSchemeAsync = await Schemes.GetDefaultSignInSchemeAsync();
                scheme = signInSchemeAsync?.Name;
                if (scheme == null)
                    throw new InvalidOperationException("No authenticationScheme was specified, and there was no DefaultSignInScheme found.");
            }
            if (!(await Handlers.GetHandlerAsync(context, scheme) is IBearerAuthenticationHandler handlerAsync))
                throw new InvalidOperationException(string.Format("No IAuthenticationSignInHandler is configured to handle sign in for the scheme: {0}", scheme));
            await handlerAsync.SignInAsync(refreshToken);
        }

        /// <summary>Sign out the specified authentication scheme.</summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Http.HttpContext" />.</param>
        /// <param name="scheme">The name of the authentication scheme.</param>
        /// <param name="properties">The <see cref="T:Microsoft.AspNetCore.Authentication.AuthenticationProperties" />.</param>
        /// <returns>A task.</returns>
        public virtual async Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            if (scheme == null)
            {
                var signOutSchemeAsync = await Schemes.GetDefaultSignOutSchemeAsync();
                scheme = signOutSchemeAsync?.Name;
                if (scheme == null)
                    throw new InvalidOperationException("No authenticationScheme was specified, and there was no DefaultSignOutScheme found.");
            }
            if (!(await Handlers.GetHandlerAsync(context, scheme) is IAuthenticationSignOutHandler handlerAsync))
                throw new InvalidOperationException(string.Format("No IAuthenticationSignOutHandler is configured to handle sign out for the scheme: {0}", scheme));
            var properties1 = properties;
            await handlerAsync.SignOutAsync(properties1);
        }
    }
}