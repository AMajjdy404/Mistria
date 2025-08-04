using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class ServiceCoverUrlResolver : IValueResolver<Service, ServiceReturnedDto, string>
    {
        private readonly IConfiguration _configuration;

        public ServiceCoverUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(Service source, ServiceReturnedDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.CoverImage))
                return $"{_configuration["BaseApiUrl"]}{source.CoverImage}";
            return string.Empty;
        }
    }
}
