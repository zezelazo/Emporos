
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Emporos.Core.Data
{
    public class UserHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserId()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx?.User?.Claims?.FirstOrDefault(t => t.Type == "uid") is null) return "";

            return _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(t=> t.Type == "uid").Value;
        }
    }
}
