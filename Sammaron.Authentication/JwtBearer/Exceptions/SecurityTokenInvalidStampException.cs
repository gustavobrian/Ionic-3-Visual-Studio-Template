using System;
using Microsoft.IdentityModel.Tokens;

namespace Sammaron.Authentication.JwtBearer.Exceptions
{
    public class SecurityTokenInvalidStampException : SecurityTokenValidationException
    {
        /// <summary>
        /// Gets or sets the InvalidIssuer that created the validation exception.
        /// </summary>
        public string InvalidIssuer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IdentityModel.Tokens.SecurityTokenInvalidIssuerException" /> class.
        /// </summary>
        public SecurityTokenInvalidStampException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IdentityModel.Tokens.SecurityTokenInvalidIssuerException" /> class.
        /// </summary>
        /// <param name="message">Addtional information to be included in the exception and displayed to user.</param>
        public SecurityTokenInvalidStampException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IdentityModel.Tokens.SecurityTokenInvalidIssuerException" /> class.
        /// </summary>
        /// <param name="message">Addtional information to be included in the exception and displayed to user.</param>
        /// <param name="innerException">A <see cref="T:System.Exception" /> that represents the root cause of the exception.</param>
        public SecurityTokenInvalidStampException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}