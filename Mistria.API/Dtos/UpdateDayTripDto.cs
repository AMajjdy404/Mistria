namespace Mistria.API.Dtos
{
    public class UpdateDayTripDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? LocationUrl { get; set; }
        public List<IFormFile>? Images { get; set; }
        public IFormFile? CoverImage { get; set; }
        public List<string>? Included { get; set; }
        public decimal? PricePerPerson { get; set; } // Nullable string
        public bool? IsMain { get; set; }
        public string? ItineraryJson { get; set; }
        public string? City { get; set; }
    }
}
