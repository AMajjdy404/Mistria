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

        public string Duration { get; set; }
        [Required]

        public List<string> Images { get; set; }
        [Required]

        public string CoverImage { get; set; }

        [Required]
        public List<string> Included { get; set; } = new List<string>();

        [Required]
        public List<string> Excluded { get; set; } = new List<string>();

        public bool? IsMain { get; set; } = false;

        public int Order { get; set; }

        public List<ItineraryDay> Itinerary { get; set; } = new List<ItineraryDay>();

        public List<PricingTier> PricingTiers { get; set; } = new List<PricingTier>();

        public decimal? GetStartingPrice()
        {
            var prices = PricingTiers?
                .SelectMany(t => t.DateRanges ?? new List<PricingDateRange>())
                .SelectMany(d => d.GroupPricing ?? new List<GroupPricing>())
                .Select(g => g.PricePerPerson)
                .ToList();

            return prices != null && prices.Count > 0 ? prices.Min() : null;
        }
    }
}
