using System.Collections.Generic;
using System.Threading.Tasks;
using Emporos.Core.Identity;
using Emporos.Core.Services;
using Emporos.Model.Dto.Auth;

namespace Emporos.Services
{
    public interface IAuthenticationService
    {
        
        Task<OperationResult> LoginAsync(LoginModel model, bool emailConfirmationEnabled);
        Task<OperationResult<string>> RegisterAsync(RegisterModel model);
        Task<OperationResult<string>> GenerateNewRegisterToken(string userEmail);
        Task<OperationResult> ConfirmEmailAsync(string token, string email);

        Task<OperationResult<List<ApplicationUser>>> GetUsers();
        Task<OperationResult<List<ApplicationUser>>> GetUsersFromRole(string role);
        Task<OperationResult<ApplicationUser>> GetUserByEmail(string email);

        Task<OperationResult> DeleteUser(string email);

        Task<OperationResult<List<string>>> GetRoles();
        Task<OperationResult<List<string>>> SearchRoles(string term);
        Task<OperationResult> CreateRole(string roleName);

        Task<OperationResult> DeleteRole(string roleName);

        Task<OperationResult> AddUserToRole(ApplicationUser user, string role);

        Task<OperationResult> RemoveUserOfRole(ApplicationUser user, string role);
    } 
}