using System.Threading.Tasks;
using Emporos.Core.Services;
using Emporos.Model.Dto.Auth;

namespace Emporos.Services
{
    public interface IPasswordService
    {
        Task<OperationResult<string>> RequestResetAsync(RequestPasswordResetModel model);
        Task<OperationResult> ResetAsync(PasswordResetModel model);
        Task<OperationResult> ChangeAsync(PasswordChangeModel model);
    }
}


