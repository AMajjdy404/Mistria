using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class WeddingCoverUrlResolver : IValueResolver<Wedding, WeddingReturnedDto, string>
    {
        private readonly IConfiguration _configuration;

        public WeddingCoverUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(Wedding source, WeddingReturnedDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.CoverImage))
                return $"{_configuration["BaseApiUrl"]}{source.CoverImage}";
            return string.Empty;
        }
    }
}
