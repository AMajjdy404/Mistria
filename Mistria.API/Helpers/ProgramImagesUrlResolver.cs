using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class ProgramImagesUrlResolver : IValueResolver<TravelProgram, ReturnedProgramDto, List<string>>
    {
        private readonly IConfiguration _configuration;

        public ProgramImagesUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<string> Resolve(TravelProgram source, ReturnedProgramDto destination, List<string> destMember, ResolutionContext context)
        {
            if (source.Images == null || !source.Images.Any())
                return new List<string>();

            var baseUrl = _configuration["BaseApiUrl"]; 

            return source.Images
                .Select(image => $"{baseUrl}{image}")
                .ToList();
        }
    }

}
