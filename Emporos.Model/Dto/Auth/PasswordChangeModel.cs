using System.ComponentModel.DataAnnotations;

namespace Emporos.Model.Dto.Auth
{
    public class PasswordChangeModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
