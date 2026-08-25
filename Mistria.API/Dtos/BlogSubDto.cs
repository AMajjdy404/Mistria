using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class BlogSubDto
    {
        [Required(ErrorMessage = "BlogId is required")]
        public int BlogId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Cover image is required")]
        public IFormFile CoverImage { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string ContentJson { get; set; }
    }
}
