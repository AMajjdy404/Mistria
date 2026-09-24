namespace Mistria.API.Dtos
{
    public class ItineraryDayReturnedDto
    {
        public int DayNumber { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public List<ItineraryEventReturnedDto> Events { get; set; }
        public List<string> Meals { get; set; }
        public string Accommodation { get; set; }
    }
}
