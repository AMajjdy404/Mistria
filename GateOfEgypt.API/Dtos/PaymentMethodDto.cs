using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class PaymentMethodDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
    }
}
