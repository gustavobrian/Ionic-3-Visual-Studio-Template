using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sammaron.Core.Models;

namespace Sammaron.Core.Data
{
    public class ApiContext : IdentityDbContext<User>
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
            
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; } 

        public DbSet<Position> Positions { get; set; }
    }
}