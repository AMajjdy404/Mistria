using System.ComponentModel.DataAnnotations;

namespace GateOfEgypt.API.Dtos
{
    public class ServiceDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Cover image is required")]
        public IFormFile CoverImage { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        public bool? IsMain { get; set; } = false;

        // Optional: 1-based display order. When omitted, the item is appended after the current last order.
        public int? Order { get; set; }
    }
}
