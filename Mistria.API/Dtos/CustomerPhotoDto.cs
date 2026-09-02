using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class CustomerPhotoDto
    {
        [Required(ErrorMessage = "Image is required")]
        public IFormFile Image { get; set; }
    }
}
