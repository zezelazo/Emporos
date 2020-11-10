using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Emporos.Core.Identity;

namespace Emporos.Data.Context
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }
    }
}
