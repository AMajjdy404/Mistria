using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class ItineraryDayImageUrlResolver : IValueResolver<ItineraryDay, ItineraryDayReturnedDto, string>
    {
        private readonly IConfiguration _configuration;

        public ItineraryDayImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(ItineraryDay source, ItineraryDayReturnedDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.Image))
                return $"{_configuration["BaseApiUrl"]}{source.Image}";
            return string.Empty;
        }
    }
}
