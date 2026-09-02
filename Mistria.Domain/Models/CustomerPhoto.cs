using System.ComponentModel.DataAnnotations;

namespace Mistria.Domain.Models
{
    public class CustomerPhoto
    {
        public int Id { get; set; }

        [Required]
        public string Image { get; set; }
    }
}
