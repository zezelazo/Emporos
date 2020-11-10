using System;
using System.Net;
using System.Threading.Tasks;
using Emporos.Core.Data;
using Emporos.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Emporos.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly Email _emailSettings;

        public EmailService(IOptions<AppSettings> options, ILogger<EmailService> logger)
        { 
            _logger = logger;
            _emailSettings = options.Value.Email;
        }

        public async ValueTask<OperationResult> SendEmail(string email, string message, string subject, bool regardsContent = true)
        {
            try
            {
                var client = new SendGridClient(_emailSettings.ApiKey);
                var from = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                var to = new EmailAddress(email);
                var htmlContent = regardsContent 
                    ? $"{_emailSettings.RegardsContent}{message}{_emailSettings.EndingContent}"
                    : message;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    _logger.LogError($"Error while sending email to '{email}' {nameof(subject)}: '{subject}'.");
                    return OperationResult.Fail($"Error while sending email.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while sending email to '{email}' {nameof(subject)}: '{subject}'.");
                return OperationResult.Fail(string.Empty);
            }

            return OperationResult.Ok();
        }
    }
}