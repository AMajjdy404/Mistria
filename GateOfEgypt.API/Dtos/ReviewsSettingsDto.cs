using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class ReviewsSettingsDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Youtube channel link is required")]
        public string YoutubeChannelLink { get; set; }

        public IFormFile? Image { get; set; }
    }
}
