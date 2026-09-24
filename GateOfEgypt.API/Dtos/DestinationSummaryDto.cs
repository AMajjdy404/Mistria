namespace GateOfEgypt.API.Dtos
{
    public class DestinationSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CoverImage { get; set; }
        public string Location { get; set; }
        public string City { get; set; }
        public int Order { get; set; }
        public decimal? StartingFromPrice { get; set; }
    }
}
