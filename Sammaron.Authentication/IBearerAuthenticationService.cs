using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Sammaron.Authentication
{
    public interface IBearerAuthenticationService : IAuthenticationService
    {
        Task SignInAsync(HttpContext context, string scheme, string refreshToken);
    }
}