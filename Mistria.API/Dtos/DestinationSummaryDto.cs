namespace Mistria.API.Dtos
{
    public class DayTripSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CoverImage { get; set; }
        public string Location { get; set; }
        public string City { get; set; }
        public decimal PricePerPerson { get; set; }
    }
}
