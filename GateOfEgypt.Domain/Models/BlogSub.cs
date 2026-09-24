using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GateOfEgypt.Domain.Models
{
    public class BlogSub
    {
        public int Id { get; set; }

        public int BlogId { get; set; }

        [Required]
        public string Title { get; set; }

        public string CoverImage { get; set; }

        public Dictionary<string, string> Content { get; set; } = new Dictionary<string, string>();
    }
}
