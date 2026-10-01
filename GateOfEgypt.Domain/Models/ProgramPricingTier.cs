namespace GateOfEgypt.Domain.Models
{
    // Pricing tier used by travel programs: each tier is priced separately with and without hotels
    public class ProgramPricingTier
    {
        // e.g. "Affordable", "Gold", "Diamond", "Platinum"
        public string Name { get; set; }
        public bool IsMostChosen { get; set; }

        // Prices and accommodation when the tour includes hotels
        public PricingOption WithHotels { get; set; } = new PricingOption();

        // Prices and accommodation when the tour is booked without hotels
        public PricingOption WithoutHotels { get; set; } = new PricingOption();
    }
}
