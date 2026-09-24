namespace GateOfEgypt.API.Dtos
{
    public class UpdateReelDto
    {
        public string? Title { get; set; }
        public string? IframeLink { get; set; }
        public IFormFile? CoverImage { get; set; }
    }
}
