﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Sammaron.Authentication.JwtBearer.Extensions;
using Sammaron.Core.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sammaron.Authentication.JwtBearer.Handlers
{
    public class BearerAuthenticationHandler : AuthenticationHandler<BearerOptions>, IAuthenticationSignInHandler
    {
        private readonly UserManager<User> _manager;

        public BearerAuthenticationHandler(IOptionsMonitor<BearerOptions> options, ILoggerFactory logger, UrlEncoder encoder,
             UserManager<User> manager, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _manager = manager;
        }

        /// <summary>
        /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
        /// If it is not provided a default instance is supplied which does nothing when the methods are called.
        /// </summary>
        protected BearerEvents BearerEvents
        {
            get => (BearerEvents)Events;
            set => Events = value;
        }

        protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new JwtBearerEvents());

        /// <summary>
        /// Searches the 'Authorization' header for a 'Bearer' token. If the 'Bearer' token is found, it is validated using <see cref="TokenValidationParameters"/> set in the options.
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var messageReceivedContext = new MessageReceivedContext(Context, Scheme, Options);

            await BearerEvents.MessageReceived(messageReceivedContext);
            if (messageReceivedContext.Result != null)
            {
                return messageReceivedContext.Result;
            }

            var validationParameters = Options.TokenValidationParameters.Clone();
            if (Options.SecurityTokenValidator.CanReadToken(messageReceivedContext.Token))
            {
                try
                {
                    var principal = Options.SecurityTokenValidator.ValidateToken(messageReceivedContext.Token, validationParameters, out var validatedToken);

                    var tokenValidatedContext = new TokenValidatedContext(Context, Scheme, Options)
                    {
                        Principal = principal,
                        SecurityToken = validatedToken
                    };

                    await BearerEvents.TokenValidated(tokenValidatedContext);
                    if (tokenValidatedContext.Result != null)
                    {
                        return tokenValidatedContext.Result;
                    }

                    if (Options.SaveToken)
                    {
                        tokenValidatedContext.Properties.StoreTokens(new[]
                        {
                                new AuthenticationToken { Name = "access_token", Value = messageReceivedContext.Token}
                            });
                    }

                    tokenValidatedContext.Success();
                    return tokenValidatedContext.Result;
                }
                catch (Exception ex)
                {
                    var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options)
                    {
                        Exception = ex
                    };

                    await BearerEvents.AuthenticationFailed(authenticationFailedContext);
                    return authenticationFailedContext.Result ?? AuthenticateResult.Fail(authenticationFailedContext.Exception);
                }
            }

            return AuthenticateResult.Fail("No SecurityTokenValidator available for token: " + messageReceivedContext.Token);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var authResult = await HandleAuthenticateOnceSafeAsync();
            var eventContext = new BearerChallengeContext(Context, Scheme, Options, properties)
            {
                AuthenticateFailure = authResult?.Failure
            };

            await BearerEvents.Challenge(eventContext);
        }

        public async Task SignOutAsync(AuthenticationProperties properties)
        {
            var user = await _manager.FindByIdAsync(properties?.Items[BearerConstants.UserIdKey]);
            if (user == null)
                return;

            await _manager.UpdateSecurityStampAsync(user);
        }

        public async Task SignInAsync(ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            var issueDate = DateTime.Now;
            var expireDate = issueDate.AddMinutes(Options.TokenLifetimeInMinutes);
            properties.IssuedUtc = issueDate;
            properties.ExpiresUtc = expireDate;
            var token = GenerateToken(principal, expireDate);
            var dictionary = new Dictionary<string, object>
            {
                { "access_token", token },
                { "token_type" , Options.Challenge},
                { "expires_in" , new TimeSpan(expireDate.Ticks).TotalSeconds}
            };
            foreach (var property in properties.Items)
            {
                var match = Regex.Match(property.Value, "({.*?})");
                dictionary.Add(property.Key, match.Success ? JsonConvert.DeserializeObject(property.Value) : property.Value);
            }
            await Response.WriteJsonAsync(dictionary);
        }

        public string GenerateToken(IPrincipal principal, DateTime expires)
        {
            var securityToken = Options.SecurityTokenValidator.CreateJwtSecurityToken(new SecurityTokenDescriptor
            {
                Issuer = Options.ClaimsIssuer,
                Audience = "*",
                SigningCredentials = new SigningCredentials(Options.TokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha512),
                EncryptingCredentials = new EncryptingCredentials(Options.TokenValidationParameters.TokenDecryptionKey, SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256),
                Subject = principal.Identity as ClaimsIdentity,
                Expires = expires
            });
            return Options.SecurityTokenValidator.WriteToken(securityToken);
        }
    }
}