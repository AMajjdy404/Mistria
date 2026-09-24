using System.Collections.Generic;

namespace GateOfEgypt.API.Dtos
{
    public class BlogSubReturnedDto
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string CoverImage { get; set; }
        public Dictionary<string, string> Content { get; set; }
    }
}
