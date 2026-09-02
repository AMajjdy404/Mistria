using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class ReviewDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "Review link is required")]
        public string ReviewLink { get; set; }

        [Required(ErrorMessage = "Cover image is required")]
        public IFormFile CoverImage { get; set; }
    }
}
