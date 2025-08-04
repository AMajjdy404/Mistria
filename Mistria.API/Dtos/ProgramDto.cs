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
        [Required(ErrorMessage = "Price Per Person is Required & Cannot Be 0")]
        [Range(1,double.MaxValue)]
        public decimal PricePerPerson { get; set; }
        public bool? IsMain { get; set; } = false;
        [Required(ErrorMessage = "Itinerary JSON is required")]
        public string ItineraryJson { get; set; }
    }
}
