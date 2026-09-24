using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class AboutUsDto
    {
        [Required(ErrorMessage = "Main description is required")]
        public string MainDescription { get; set; }

        [Required(ErrorMessage = "Our story is required")]
        public string OurStory { get; set; }
    }
}
