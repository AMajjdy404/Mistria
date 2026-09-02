using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class ReviewPlatformDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Reviews count label is required")]
        public string ReviewsCountLabel { get; set; }

        [Required(ErrorMessage = "Link is required")]
        public string Link { get; set; }

        [Required(ErrorMessage = "Icon is required")]
        public IFormFile Icon { get; set; }
    }
}
