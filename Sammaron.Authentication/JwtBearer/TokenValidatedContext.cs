using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Sammaron.Authentication.JwtBearer
{
    public class TokenValidatedContext : ResultContext<BearerOptions>
    {
        public TokenValidatedContext(HttpContext context, AuthenticationScheme scheme, BearerOptions options)
            : base(context, scheme, options)
        {
        }

        public SecurityToken SecurityToken { get; set; }
    }
}