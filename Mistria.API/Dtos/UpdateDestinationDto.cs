namespace Mistria.API.Dtos
{
    public class UpdateDestinationDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Duration { get; set; }
        public List<IFormFile>? Images { get; set; }
        public IFormFile? CoverImage { get; set; }
        public List<string>? Included { get; set; }
        public List<string>? Excluded { get; set; }
        public bool? IsMain { get; set; }
        public int? Order { get; set; }
        public string? ItineraryJson { get; set; }
        public List<IFormFile>? ItineraryDayImages { get; set; }
        public List<int>? ItineraryDayImageIndexes { get; set; }
        public string? PricingTiersJson { get; set; }
        public string? City { get; set; }
    }
}
