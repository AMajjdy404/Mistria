namespace GateOfEgypt.API.Dtos
{
    public class UpdateSocialMediaLinkDto
    {
        public string? Title { get; set; }
        public string? Link { get; set; }
        public IFormFile? Icon { get; set; }
    }
}
