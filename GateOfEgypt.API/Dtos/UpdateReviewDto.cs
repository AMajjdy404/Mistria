namespace Mistria.API.Dtos
{
    public class UpdateReviewDto
    {
        public string? Name { get; set; }
        public string? Country { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string? ReviewLink { get; set; }
        public IFormFile? CoverImage { get; set; }
    }
}
