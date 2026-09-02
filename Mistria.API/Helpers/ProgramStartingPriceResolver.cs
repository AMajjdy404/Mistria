using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class ProgramStartingPriceResolver : IValueResolver<TravelProgram, ReturnedProgramDto, decimal?>
    {
        public decimal? Resolve(TravelProgram source, ReturnedProgramDto destination, decimal? destMember, ResolutionContext context)
        {
            return source.GetStartingPrice();
        }
    }
}
