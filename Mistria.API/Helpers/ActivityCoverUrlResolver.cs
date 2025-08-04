using AutoMapper;
using AutoMapper.Execution;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class ActivityCoverUrlResolver : IValueResolver<Activity, ActivityReturnedDto, string>
    {
        private readonly IConfiguration _configuration;

        public ActivityCoverUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(Activity source, ActivityReturnedDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.CoverImage))
                return $"{_configuration["BaseApiUrl"]}{source.CoverImage}";
            return string.Empty;
        }
    }
}
