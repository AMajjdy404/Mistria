using Mistria.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Mistria.API.Dtos
{
    public class ReturnedProgramDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Duration { get; set; }
        public List<string> Images { get; set; }
        public string CoverImage { get; set; }
        public List<string> Included { get; set; }
        public List<string> Excluded { get; set; }
        public bool IsMain { get; set; }
        public int Order { get; set; }
        public List<ItineraryDayReturnedDto> Itinerary { get; set; }
        public List<PricingTier> PricingTiers { get; set; }
        public decimal? StartingFromPrice { get; set; }
    }
}
