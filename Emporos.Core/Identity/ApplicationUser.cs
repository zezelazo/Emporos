using System;
using Microsoft.AspNetCore.Identity;

namespace Emporos.Core.Identity
{
    public class ApplicationUser : IdentityUser, IAppUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime SignedIn { get; set; }
        public DateTime LastDateContentView { get; set; }
        public string Salt { get; set; }
    }
}
