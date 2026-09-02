using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Mistria.API.Dtos
{
    public class ProgramDto
    {
        [Required(ErrorMessage ="Title is Required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is Required")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Location is Required")]
        public string Location { get; set; }
        [Required(ErrorMessage = "LocationUrl is Required")]
        public string LocationUrl { get; set; }
        [Required(ErrorMessage = "Images is Required")]
        public List<IFormFile> Images { get; set; }
        [Required(ErrorMessage = "Cover Image is Required")]
        public IFormFile CoverImage { get; set; }

        [Required(ErrorMessage = "Included is Required")]
        public List<string> Included { get; set; }
        [Required(ErrorMessage = "Excluded is Required")]
        public List<string> Excluded { get; set; }

        public bool? IsMain { get; set; } = false;

        [Required(ErrorMessage = "Itinerary JSON is required")]
        public string ItineraryJson { get; set; }

        // Optional: cover photo per itinerary day. ItineraryDayImageIndexes[i] gives the
        // zero-based index into the parsed Itinerary array that ItineraryDayImages[i] belongs to.
        public List<IFormFile>? ItineraryDayImages { get; set; }
        public List<int>? ItineraryDayImageIndexes { get; set; }

        [Required(ErrorMessage = "Pricing tiers JSON is required")]
        public string PricingTiersJson { get; set; }
    }
}
