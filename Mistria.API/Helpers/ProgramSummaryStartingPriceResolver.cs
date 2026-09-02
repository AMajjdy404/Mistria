using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class ProgramSummaryStartingPriceResolver : IValueResolver<TravelProgram, TravelProgramSummaryDto, decimal?>
    {
        public decimal? Resolve(TravelProgram source, TravelProgramSummaryDto destination, decimal? destMember, ResolutionContext context)
        {
            return source.GetStartingPrice();
        }
    }
}
