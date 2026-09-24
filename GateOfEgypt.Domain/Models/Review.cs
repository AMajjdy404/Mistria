using System.ComponentModel.DataAnnotations;

namespace GateOfEgypt.Domain.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required]
        public string ReviewLink { get; set; }

        public string CoverImage { get; set; }
    }
}
