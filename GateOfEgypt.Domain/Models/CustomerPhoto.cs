using System.ComponentModel.DataAnnotations;

namespace GateOfEgypt.Domain.Models
{
    public class CustomerPhoto
    {
        public int Id { get; set; }

        [Required]
        public string Image { get; set; }
    }
}
