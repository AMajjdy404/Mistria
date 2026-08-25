using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistria.Domain.Models
{
    public class DayTrip
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string LocationUrl { get; set; }

        public List<string> Images { get; set; }

        public string CoverImage { get; set; }

        public List<string> Included { get; set; }

        [Required]
        public decimal PricePerPerson { get; set; } 

        [Required]
        public bool IsMain { get; set; }

        public Dictionary<string, string> Itinerary { get; set; }
        [Required]
        public string City { get; set; }
    }
}
