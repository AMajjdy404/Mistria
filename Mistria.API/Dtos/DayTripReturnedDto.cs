namespace Mistria.API.Dtos
{
    public class DayTripReturnedDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string LocationUrl { get; set; }
        public List<string> Images { get; set; }
        public string CoverImage { get; set; }
        public List<string> Included { get; set; }
        public decimal PricePerPerson { get; set; } 
        public bool IsMain { get; set; }
        public Dictionary<string, string> Itinerary { get; set; }
        public string City { get; set; }
    }
}
