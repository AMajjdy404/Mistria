using System.ComponentModel.DataAnnotations;

namespace Mistria.Domain.Models
{
    public class ReviewPlatform
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        // Free-text label, e.g. "5,425+ reviews" or "+80"
        [Required]
        public string ReviewsCountLabel { get; set; }

        [Required]
        public string Link { get; set; }

        public string Icon { get; set; }
    }
}
