namespace GateOfEgypt.API.Dtos
{
    public class UpdateBlogSubDto
    {
        public string? Title { get; set; }
        public IFormFile? CoverImage { get; set; }

        // Set to true (with no CoverImage) to clear the existing image. Ignored if CoverImage is provided.
        public bool? RemoveCoverImage { get; set; }
        public string? ContentJson { get; set; }
    }
}
