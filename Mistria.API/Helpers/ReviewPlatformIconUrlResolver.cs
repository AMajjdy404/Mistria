using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class ReviewPlatformIconUrlResolver : IValueResolver<ReviewPlatform, ReviewPlatformReturnedDto, string>
    {
        private readonly IConfiguration _configuration;

        public ReviewPlatformIconUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(ReviewPlatform source, ReviewPlatformReturnedDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.Icon))
                return $"{_configuration["BaseApiUrl"]}{source.Icon}";
            return string.Empty;
        }
    }
}
