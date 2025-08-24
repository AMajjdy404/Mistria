using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class EmailDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string Phone { get; set; }

        public string? Title { get; set; }
        public int NumberOfPeople { get; set; } = 1;
        public string? Message { get; set; }
    }
}
