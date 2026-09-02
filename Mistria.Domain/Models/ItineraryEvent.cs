namespace Mistria.Domain.Models
{
    public class ItineraryEvent
    {
        public string Title { get; set; }
        public string Description { get; set; }

        // Drives the icon shown on the frontend, e.g. "meal", "overnight", "activity", "transfer"
        public string Type { get; set; }
    }
}
