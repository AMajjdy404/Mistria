using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistria.Domain.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string CoverImage { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
