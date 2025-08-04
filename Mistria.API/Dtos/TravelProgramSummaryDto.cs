namespace Mistria.API.Dtos
{
    public class TravelProgramSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CoverImage { get; set; }
        public string Location { get; set; }
        public decimal PricePerPerson { get; set; }
    }
}
