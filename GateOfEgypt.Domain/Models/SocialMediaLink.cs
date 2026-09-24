using System.ComponentModel.DataAnnotations;

namespace GateOfEgypt.Domain.Models
{
    public class SocialMediaLink
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Link { get; set; }

        public string Icon { get; set; }
    }
}
