using System.ComponentModel.DataAnnotations;

namespace GateOfEgypt.Domain.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
