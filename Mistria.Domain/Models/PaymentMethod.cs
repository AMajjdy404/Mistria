using System.ComponentModel.DataAnnotations;

namespace Mistria.Domain.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
