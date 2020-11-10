using System.ComponentModel.DataAnnotations;

namespace Emporos.Model.Dto.Auth
{
    public class PasswordResetModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
