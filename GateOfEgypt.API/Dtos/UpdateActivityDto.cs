namespace Mistria.API.Dtos
{
    public class UpdateActivityDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? CoverImage { get; set; }
        public decimal? Price { get; set; }
    }
}
