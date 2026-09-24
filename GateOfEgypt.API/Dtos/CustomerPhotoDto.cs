using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class CustomerPhotoDto
    {
        [Required(ErrorMessage = "Images are required")]
        public List<IFormFile> Images { get; set; }
    }
}
