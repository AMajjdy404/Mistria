using System.Collections.Generic;

namespace Mistria.Domain.Models
{
    public class ItineraryDay
    {
        public int DayNumber { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public List<ItineraryEvent> Events { get; set; } = new List<ItineraryEvent>();
        public List<string> Meals { get; set; } = new List<string>();
        public string Accommodation { get; set; }
    }
}
