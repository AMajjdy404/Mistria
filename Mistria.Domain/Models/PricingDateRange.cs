using System;
using System.Collections.Generic;

namespace Mistria.Domain.Models
{
    public class PricingDateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<GroupPricing> GroupPricing { get; set; } = new List<GroupPricing>();
    }
}
