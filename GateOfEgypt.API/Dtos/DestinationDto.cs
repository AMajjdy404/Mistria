using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace GateOfEgypt.API.Dtos
{
    public class DestinationDto
    {
        [Required(ErrorMessage = "Title is Required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is Required")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Location is Required")]
        public string Location { get; set; }
        [Required(ErrorMessage = "Duration is Required")]
        public string Duration { get; set; }
        [Required(ErrorMessage = "Images is Required")]
        public List<IFormFile> Images { get; set; }
        [Required(ErrorMessage = "Cover Image is Required")]
        public IFormFile CoverImage { get; set; }

        [Required(ErrorMessage = "Included is Required")]
        public List<string> Included { get; set; }
        [Required(ErrorMessage = "Excluded is Required")]
        public List<string> Excluded { get; set; }

        public bool? IsMain { get; set; } = false;

        // Optional: 1-based display order. When omitted, the item is appended after the current last order.
        public int? Order { get; set; }

        [Required(ErrorMessage = "Itinerary JSON is required")]
        public string ItineraryJson { get; set; }

        // Optional: cover photo per itinerary day. ItineraryDayImageIndexes[i] gives the
        // zero-based index into the parsed Itinerary array that ItineraryDayImages[i] belongs to.
        public List<IFormFile>? ItineraryDayImages { get; set; }
        public List<int>? ItineraryDayImageIndexes { get; set; }

        [Required(ErrorMessage = "Pricing tiers JSON is required")]
        public string PricingTiersJson { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
    }
}
