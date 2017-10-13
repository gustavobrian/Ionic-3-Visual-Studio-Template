using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Sammaron.Core.Interfaces;

namespace Sammaron.Core.Models
{
    public class User : IdentityUser, IEntity<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Headline { get; set; }
        public string Summary { get; set; }
        public string ProfileImageUrl { get; set; }
        public virtual ICollection<Position> Positions { get; set; }
    }
}