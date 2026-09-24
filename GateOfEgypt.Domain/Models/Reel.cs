using System.ComponentModel.DataAnnotations;

namespace GateOfEgypt.Domain.Models
{
    public class Reel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string IframeLink { get; set; }

        public string CoverImage { get; set; }
    }
}
