using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class EventDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Cover image is required")]
        public IFormFile CoverImage { get; set; }
    }
}
