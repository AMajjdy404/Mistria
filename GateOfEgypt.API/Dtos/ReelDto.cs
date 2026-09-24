using System.ComponentModel.DataAnnotations;

namespace GateOfEgypt.API.Dtos
{
    public class ReelDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Iframe link is required")]
        public string IframeLink { get; set; }

        public IFormFile? CoverImage { get; set; }
    }
}
