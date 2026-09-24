using System.ComponentModel.DataAnnotations;

namespace GateOfEgypt.Domain.Models
{
    public class Founder
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string CoverImage { get; set; }
    }
}
