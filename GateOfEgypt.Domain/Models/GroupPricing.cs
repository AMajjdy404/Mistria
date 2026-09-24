namespace GateOfEgypt.Domain.Models
{
    public class GroupPricing
    {
        // Free-text bracket label, e.g. "9-16 Pax", "5-8 Pax", "2-4 Pax", "Solo"
        public string GroupSizeLabel { get; set; }
        public decimal PricePerPerson { get; set; }
    }
}
