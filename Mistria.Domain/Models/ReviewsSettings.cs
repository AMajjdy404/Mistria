using System.ComponentModel.DataAnnotations;

namespace Mistria.Domain.Models
{
    public class ReviewsSettings
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Image { get; set; }

        [Required]
        public string YoutubeChannelLink { get; set; }
    }
}
