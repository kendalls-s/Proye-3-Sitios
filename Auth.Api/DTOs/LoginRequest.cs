using System.ComponentModel.DataAnnotations;

namespace Auth.Api.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}