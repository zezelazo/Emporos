using Microsoft.AspNetCore.Identity;
using Emporos.Core.Data;
using Emporos.Core.Identity;
using Emporos.Data.Context; 
using System.Linq;

namespace Emporos.Data
{
    public class DataSeeder : IDataSeeder
    {
        private readonly IdentityDbContext _identityCtx;
        private readonly EmporosContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataSeeder(  IdentityDbContext identityCtx,
            UserManager<ApplicationUser> userManager, EmporosContext ctx, RoleManager<IdentityRole> roleManager)
        {
            _identityCtx = identityCtx;
            _ctx = ctx;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Seed()
        {
            if (!_identityCtx.Users.Any(u=>u.Email == "siteadmin@emporostest.com"))
            {

                 _roleManager.CreateAsync(new IdentityRole("mega-admin")).GetAwaiter().GetResult();
                 _roleManager.CreateAsync(new IdentityRole("user")).GetAwaiter().GetResult(); 
                ;
                var admin = new ApplicationUser
                { 
                    Email = "siteadmin@emporostest.com",
                    FirstName = "Emporos",
                    LastName = "Admin",
                };
                 
                var result = _userManager.CreateAsync(admin, "zaq1@WSX").GetAwaiter().GetResult();

                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(admin, "mega-admin").GetAwaiter().GetResult();
                    var token = _userManager.GenerateEmailConfirmationTokenAsync(admin).GetAwaiter().GetResult();
                    _userManager.ConfirmEmailAsync(admin, token).GetAwaiter().GetResult();
                }

               
            }

        }
    }
}
