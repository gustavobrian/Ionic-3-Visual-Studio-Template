using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Sammaron.Authentication.JwtBearer
{
    public class MessageReceivedContext : ResultContext<BearerOptions>
    {
        public MessageReceivedContext(HttpContext context, AuthenticationScheme scheme, BearerOptions options)
            : base(context, scheme, options)
        {
        }

        /// <summary>
        /// Bearer Token. This will give application an opportunity to retrieve token from an alternation location.
        /// </summary>
        public string Token { get; set; }
    }
}