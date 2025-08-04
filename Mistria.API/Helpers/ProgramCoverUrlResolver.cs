using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class ProgramCoverUrlResolver : IValueResolver<TravelProgram, ReturnedProgramDto, string>
    {
        private readonly IConfiguration _configuration;

        public ProgramCoverUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(TravelProgram source, ReturnedProgramDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.CoverImage))
                return $"{_configuration["BaseApiUrl"]}{source.CoverImage}";
            return string.Empty;
        }
    }
}
