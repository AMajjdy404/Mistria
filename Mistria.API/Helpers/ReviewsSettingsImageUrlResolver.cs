using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class ReviewsSettingsImageUrlResolver : IValueResolver<ReviewsSettings, ReviewsSettingsReturnedDto, string>
    {
        private readonly IConfiguration _configuration;

        public ReviewsSettingsImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(ReviewsSettings source, ReviewsSettingsReturnedDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.Image))
                return $"{_configuration["BaseApiUrl"]}{source.Image}";
            return string.Empty;
        }
    }
}
