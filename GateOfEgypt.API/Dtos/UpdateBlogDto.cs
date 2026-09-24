namespace GateOfEgypt.API.Dtos
{
    public class UpdateBlogDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? CoverImage { get; set; }
    }
}
