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

        // Optional. Any subs not referenced by SubImageIndexes are created with no image.
        public List<IFormFile>? SubImages { get; set; }

        // Required alongside SubImages: SubImageIndexes[i] is the zero-based index into the
        // parsed Subs array that SubImages[i] belongs to (e.g. [0, 2] to give sub 0 and sub 2
        // an image while leaving sub 1 without one).
        public List<int>? SubImageIndexes { get; set; }
    }
}
