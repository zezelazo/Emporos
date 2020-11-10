﻿using System.ComponentModel.DataAnnotations;

namespace Emporos.Model.Dto.Auth
{
    public class LoginModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
