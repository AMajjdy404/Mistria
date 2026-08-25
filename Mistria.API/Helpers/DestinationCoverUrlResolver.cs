using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class DayTripCoverUrlResolver : IValueResolver<DayTrip, DayTripReturnedDto, string>
    {
        private readonly IConfiguration _configuration;

        public DayTripCoverUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(DayTrip source, DayTripReturnedDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.CoverImage))
                return $"{_configuration["BaseApiUrl"]}{source.CoverImage}";
            return string.Empty;
        }
    }
}
