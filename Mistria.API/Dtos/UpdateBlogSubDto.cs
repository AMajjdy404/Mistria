namespace Mistria.API.Dtos
{
    public class UpdateBlogSubDto
    {
        public string? Title { get; set; }
        public IFormFile? CoverImage { get; set; }
        public string? ContentJson { get; set; }
    }
}
