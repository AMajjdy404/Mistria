namespace Mistria.API.Dtos
{
    public class UpdateWeddingDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? CoverImage { get; set; }
    }
}
