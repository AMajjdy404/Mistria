using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class DayTripImagesUrlResolver : IValueResolver<DayTrip, DayTripReturnedDto, List<string>>
    {
        private readonly IConfiguration _configuration;

        public DayTripImagesUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<string> Resolve(DayTrip source, DayTripReturnedDto destination, List<string> destMember, ResolutionContext context)
        {
            if (source.Images == null || !source.Images.Any())
                return new List<string>();

            var baseUrl = _configuration["BaseApiUrl"];

            return source.Images
                .Select(image => $"{baseUrl}{image}")
                .ToList();
        }
    }
}
