using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class DayTripDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Location URL is required")]
        public string LocationUrl { get; set; }

        [Required(ErrorMessage = "Images are required")]
        public List<IFormFile> Images { get; set; }

        [Required(ErrorMessage = "Cover image is required")]
        public IFormFile CoverImage { get; set; }

        [Required(ErrorMessage = "Included items are required")]
        public List<string> Included { get; set; }

        [Required(ErrorMessage = "Price per person is required")]
        public decimal PricePerPerson { get; set; } 

        [Required(ErrorMessage = "IsMain is required")]
        public bool IsMain { get; set; }

        [Required(ErrorMessage = "Itinerary JSON is required")]
        public string ItineraryJson { get; set; }
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
    }
}
