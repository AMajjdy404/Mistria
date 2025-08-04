using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class EventCoverUrlResolver:IValueResolver<Event,EventReturnedDto,string>
    {
        private readonly IConfiguration _configuration;

        public EventCoverUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Event source, EventReturnedDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.CoverImage))
                return $"{_configuration["BaseApiUrl"]}{source.CoverImage}";
            return string.Empty;
        }
    }
}
