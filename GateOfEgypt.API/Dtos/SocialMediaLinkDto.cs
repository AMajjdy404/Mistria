using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class SocialMediaLinkDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Link is required")]
        public string Link { get; set; }

        [Required(ErrorMessage = "Icon is required")]
        public IFormFile Icon { get; set; }
    }
}
