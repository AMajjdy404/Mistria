using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class LoginDto
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }
}
