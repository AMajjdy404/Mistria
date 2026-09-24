using System.Collections.Generic;

namespace GateOfEgypt.Domain.Models
{
    public class PricingTier
    {
        // e.g. "Affordable", "Gold", "Diamond", "Platinum"
        public string Name { get; set; }
        public bool IsMostChosen { get; set; }
        public List<PricingDateRange> DateRanges { get; set; } = new List<PricingDateRange>();
    }
}
