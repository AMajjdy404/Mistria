using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class MappingProfiles: Profile
    {
        public MappingProfiles()
        {
            CreateMap<TravelProgram,ReturnedProgramDto>()
                .ForMember(d => d.Images, O => O.MapFrom<ProgramImagesUrlResolver>()) 
                .ForMember(d => d.CoverImage, O => O.MapFrom<ProgramCoverUrlResolver>());

            CreateMap<Destination, DestinationReturnedDto>()
            .ForMember(d => d.Images, o => o.MapFrom<DestinationImagesUrlResolver>())
            .ForMember(d => d.CoverImage, o => o.MapFrom<DestinationCoverUrlResolver>());

            CreateMap<Wedding, WeddingReturnedDto>()
            .ForMember(d => d.CoverImage, O => O.MapFrom<WeddingCoverUrlResolver>());

            CreateMap<Event, EventReturnedDto>()
            .ForMember(d => d.CoverImage, o => o.MapFrom<EventCoverUrlResolver>());

            CreateMap<Activity, ActivityReturnedDto>()
            .ForMember(d => d.CoverImage, o => o.MapFrom<ActivityCoverUrlResolver>());

            CreateMap<Service, ServiceReturnedDto>()
            .ForMember(d => d.CoverImage, o => o.MapFrom<ServiceCoverUrlResolver>());

            CreateMap<AboutUs, AboutUsReturnedDto>();

            CreateMap<Founder, FounderReturnedDto>()
            .ForMember(d => d.CoverImage, o => o.MapFrom<FounderCoverUrlResolver>());

            CreateMap<Blog, BlogReturnedDto>()
            .ForMember(d => d.CoverImage, o => o.MapFrom<BlogCoverUrlResolver>());

            CreateMap<BlogSub, BlogSubReturnedDto>()
            .ForMember(d => d.CoverImage, o => o.MapFrom<BlogSubCoverUrlResolver>());

            CreateMap<PaymentMethod, PaymentMethodReturnedDto>();

            CreateMap<TravelProgram, TravelProgramSummaryDto>()
           .ForMember(d => d.Id, o => o.MapFrom(src => src.Id))
           .ForMember(d => d.Title, o => o.MapFrom(src => src.Title))
           .ForMember(d => d.CoverImage, o => o.MapFrom(src => src.CoverImage))
           .ForMember(d => d.Location, o => o.MapFrom(src => src.Location))
           .ForMember(d => d.PricePerPerson, o => o.MapFrom(src => src.PricePerPerson));

            CreateMap<Destination, DestinationSummaryDto>()
            .ForMember(d => d.Id, o => o.MapFrom(src => src.Id))
            .ForMember(d => d.Title, o => o.MapFrom(src => src.Title))
            .ForMember(d => d.CoverImage, o => o.MapFrom(src => src.CoverImage))
            .ForMember(d => d.Location, o => o.MapFrom(src => src.Location))
            .ForMember(d => d.City, o => o.MapFrom(src => src.City))
            .ForMember(d => d.PricePerPerson, o => o.MapFrom(src => src.PricePerPerson));
        }
    }
}
