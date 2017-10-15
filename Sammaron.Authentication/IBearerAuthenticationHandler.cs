using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Sammaron.Authentication
{
    public interface IBearerAuthenticationHandler : IAuthenticationSignInHandler
    {
        Task SignInAsync(string refreshToken);
    }
}