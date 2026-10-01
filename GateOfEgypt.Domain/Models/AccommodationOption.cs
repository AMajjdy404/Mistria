namespace GateOfEgypt.Domain.Models
{
    public class AccommodationOption
    {
        // City / stay type shown as the title, e.g. "Cairo", "Luxor", "Cruise"
        public string Title { get; set; }

        // Hotels offered there, e.g. "Triumph Plaza or Barcelo Or Pyramids Park Resort or similar."
        public string Description { get; set; }
    }
}
