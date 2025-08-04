using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistria.Domain.Models
{
    public class TravelProgram
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
        [Required]

        public List<string> Images { get; set; }
        [Required]

        public string CoverImage { get; set; }
   
        [Required]

        public List<string> Included { get; set; }
        [Required]

        public decimal PricePerPerson { get; set; }
        
        public bool? IsMain { get; set; } = false;
        [Required]
        public Dictionary<string, string> Itinerary { get; set; } = new Dictionary<string, string>();
    }
}
