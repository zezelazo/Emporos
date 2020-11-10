using System.Threading.Tasks;
using Emporos.Core.Services;

namespace Emporos.Services
{
    public interface IEmailService
    {
        ValueTask<OperationResult> SendEmail(string email, string message, string subject, bool regardsContent = true);
    }
}
