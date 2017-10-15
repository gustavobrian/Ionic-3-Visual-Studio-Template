using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace Sammaron.Authentication.JwtBearer
{
    public class BearerOptions : BearerOptions<JwtSecurityTokenHandler>
    {
        
    }

    public class BearerOptions<TSecurityTokenValidator> : AuthenticationSchemeOptions where TSecurityTokenValidator : class, ISecurityTokenValidator, new()
    {
        public const int DefaultTokenLifetimeInMinutes = 1560;

        /// <summary>
        /// Gets or sets if HTTPS is required for the metadata address or authority.
        /// The default is true. This should be disabled only in development environments.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets the audience for any received token.
        /// </summary>
        /// <value>
        /// The expected audience for any received token.
        /// </value>
        public string Audience { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int TokenLifetimeInMinutes { get; set; } = DefaultTokenLifetimeInMinutes;

        /// <summary>
        /// Gets or sets the challenge to put in the "WWW-Authenticate" header.
        /// </summary>
        public string Challenge { get; set; } = "Bearer";

        /// <summary>
        /// The object provided by the application to process events raised by the bearer authentication handler.
        /// The application may implement the interface fully, or it may create an instance of JwtBearerEvents
        /// and assign delegates only to the events it wants to process.
        /// </summary>
        public BearerEvents BearerEvents
        {
            get => (BearerEvents)Events;
            set => Events = value;
        }

        /// <summary>
        /// Gets the ordered list of <see cref="T:Microsoft.IdentityModel.Tokens.ISecurityTokenValidator" /> used to validate access tokens.
        /// </summary>
        public TSecurityTokenValidator SecurityTokenValidator { get; } = new TSecurityTokenValidator();

        /// <summary>
        /// Gets or sets the parameters used to validate identity tokens.
        /// </summary>
        /// <remarks>Contains the types and definitions required for validating a token.</remarks>
        /// <exception cref="T:System.ArgumentNullException">if 'value' is null.</exception>
        public TokenValidationParameters TokenValidationParameters { get; set; } = new TokenValidationParameters();

        /// <summary>
        /// Defines whether the bearer token should be stored in the
        /// <see cref="T:Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties" /> after a successful authorization.
        /// </summary>
        public bool SaveToken { get; set; } = true;

        /// <summary>
        /// Defines whether the token validation errors should be returned to the caller.
        /// Enabled by default, this option can be disabled to prevent the JWT handler
        /// from returning an error and an error_description in the WWW-Authenticate header.
        /// </summary>
        public bool IncludeErrorDetails { get; set; } = true;
    }
}