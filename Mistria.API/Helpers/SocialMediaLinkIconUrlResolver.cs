using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class SocialMediaLinkIconUrlResolver : IValueResolver<SocialMediaLink, SocialMediaLinkReturnedDto, string>
    {
        private readonly IConfiguration _configuration;

        public SocialMediaLinkIconUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(SocialMediaLink source, SocialMediaLinkReturnedDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.Icon))
                return $"{_configuration["BaseApiUrl"]}{source.Icon}";
            return string.Empty;
        }
    }
}
