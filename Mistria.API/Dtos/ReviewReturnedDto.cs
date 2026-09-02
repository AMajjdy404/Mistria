namespace Mistria.API.Dtos
{
    public class ReviewReturnedDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string ReviewLink { get; set; }
        public string CoverImage { get; set; }
    }
}
