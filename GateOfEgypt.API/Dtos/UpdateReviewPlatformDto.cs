namespace GateOfEgypt.API.Dtos
{
    public class UpdateReviewPlatformDto
    {
        public string? Name { get; set; }
        public string? ReviewsCountLabel { get; set; }
        public string? Link { get; set; }
        public IFormFile? Icon { get; set; }
    }
}
