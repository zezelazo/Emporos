
namespace Emporos.Core.Data
{
    public class Email
    {
        public bool EmailConfirmationEnabled { get; set; } 
        public string ApiKey { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string Subject_Welcome { get; set; }
        public string RegardsContent { get; set; }
        public string HtmlContent_Welcome { get; set; }
        public string HtmlContent_Welcome1 { get; set; }
        public string EndingContent { get; set; }
        public string Subject_ResetPassword { get; set; }
        public string HtmlContent_ResetPassword { get; set; }
        public string HtmlContent_ResetPasswordEnding { get; set; }
        public string UIServer { get; set; }
    }
}