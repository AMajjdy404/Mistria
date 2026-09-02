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
                .ForMember(d => d.CoverImage, O => O.MapFrom<ProgramCoverUrlResolver>())
                .ForMember(d => d.StartingFromPrice, o => o.MapFrom<ProgramStartingPriceResolver>());

            CreateMap<ItineraryDay, ItineraryDayReturnedDto>()
            .ForMember(d => d.Image, o => o.MapFrom<ItineraryDayImageUrlResolver>());

            CreateMap<ItineraryEvent, ItineraryEventReturnedDto>();

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

            CreateMap<Review, ReviewReturnedDto>()
            .ForMember(d => d.CoverImage, o => o.MapFrom<ReviewCoverUrlResolver>());

            CreateMap<ReviewPlatform, ReviewPlatformReturnedDto>()
            .ForMember(d => d.Icon, o => o.MapFrom<ReviewPlatformIconUrlResolver>());

            CreateMap<Reel, ReelReturnedDto>()
            .ForMember(d => d.CoverImage, o => o.MapFrom<ReelCoverUrlResolver>());

            CreateMap<CustomerPhoto, CustomerPhotoReturnedDto>()
            .ForMember(d => d.Image, o => o.MapFrom<CustomerPhotoImageUrlResolver>());

            CreateMap<ReviewsSettings, ReviewsSettingsReturnedDto>()
            .ForMember(d => d.Image, o => o.MapFrom<ReviewsSettingsImageUrlResolver>());

            CreateMap<SocialMediaLink, SocialMediaLinkReturnedDto>()
            .ForMember(d => d.Icon, o => o.MapFrom<SocialMediaLinkIconUrlResolver>());

            CreateMap<TravelProgram, TravelProgramSummaryDto>()
           .ForMember(d => d.Id, o => o.MapFrom(src => src.Id))
           .ForMember(d => d.Title, o => o.MapFrom(src => src.Title))
           .ForMember(d => d.CoverImage, o => o.MapFrom(src => src.CoverImage))
           .ForMember(d => d.Location, o => o.MapFrom(src => src.Location))
           .ForMember(d => d.StartingFromPrice, o => o.MapFrom<ProgramSummaryStartingPriceResolver>());

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
