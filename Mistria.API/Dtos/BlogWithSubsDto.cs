using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class BlogWithSubsDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Cover image is required")]
        public IFormFile CoverImage { get; set; }

        // JSON array of { "Title": "...", "Content": { "key": "value", ... } }
        [Required(ErrorMessage = "Subs JSON is required")]
        public string SubsJson { get; set; }

        // One cover image per sub, in the same order as the parsed Subs array
        [Required(ErrorMessage = "Sub images are required")]
        public List<IFormFile> SubImages { get; set; }
    }
}
