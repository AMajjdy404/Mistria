namespace Mistria.API.Dtos
{
    public class UpdateEventDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? CoverImage { get; set; }
    }
}
