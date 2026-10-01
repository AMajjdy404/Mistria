using System.Collections.Generic;

namespace GateOfEgypt.Domain.Models
{
    public class PricingOption
    {
        public List<PricingDateRange> DateRanges { get; set; } = new List<PricingDateRange>();
        public List<AccommodationOption> AccommodationOptions { get; set; } = new List<AccommodationOption>();
    }
}
