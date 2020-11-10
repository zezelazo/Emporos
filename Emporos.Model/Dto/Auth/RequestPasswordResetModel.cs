using System.ComponentModel.DataAnnotations;

namespace Emporos.Model.Dto.Auth
{
    public class RequestPasswordResetModel
    {
        [Required]
        public string Email { get; set; }
    }
}
