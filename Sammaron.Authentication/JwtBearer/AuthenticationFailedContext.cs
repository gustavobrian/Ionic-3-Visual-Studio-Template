using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Sammaron.Authentication.JwtBearer
{
    public class AuthenticationFailedContext : ResultContext<BearerOptions>
    {
        public AuthenticationFailedContext(HttpContext context, AuthenticationScheme scheme, BearerOptions options)
            : base(context, scheme, options)
        {
        }

        public Exception Exception { get; set; }
    }
}