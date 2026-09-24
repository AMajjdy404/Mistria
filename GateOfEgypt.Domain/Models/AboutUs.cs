using System.ComponentModel.DataAnnotations;

namespace Mistria.Domain.Models
{
    public class AboutUs
    {
        public int Id { get; set; }

        [Required]
        public string MainDescription { get; set; }

        [Required]
        public string OurStory { get; set; }
    }
}
