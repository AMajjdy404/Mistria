using System.ComponentModel.DataAnnotations;

namespace Mistria.Domain.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string CoverImage { get; set; }
    }
}
