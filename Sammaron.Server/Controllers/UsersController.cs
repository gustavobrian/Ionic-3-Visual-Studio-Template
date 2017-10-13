using Microsoft.AspNetCore.Mvc;
using Sammaron.Core.Data;
using Sammaron.Core.Models;

namespace Sammaron.Server.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : BaseController<User, string>
    {
        public UsersController(ApiContext context) : base(context)
        {
            LightSelector = u => new User
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Summary = u.Summary,
                ProfileImageUrl = u.ProfileImageUrl
            };

            DetailsSelector = u => new User
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                ProfileImageUrl = u.ProfileImageUrl,
                Positions = u.Positions
            };
        }
    }
}