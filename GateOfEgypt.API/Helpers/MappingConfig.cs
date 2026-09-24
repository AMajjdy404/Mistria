using Mapster;
using GateOfEgypt.API.Dtos;
using GateOfEgypt.Domain.Models;

namespace GateOfEgypt.API.Helpers
{
    public static class MappingConfig
    {
        public static void Configure()
        {
            TypeAdapterConfig<TravelProgram, ReturnedProgramDto>.NewConfig()
                .Map(d => d.Images, s => UrlHelper.PrefixAll(s.Images))
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage))
                .Map(d => d.StartingFromPrice, s => s.GetStartingPrice());

            TypeAdapterConfig<TravelProgram, TravelProgramSummaryDto>.NewConfig()
                .Map(d => d.StartingFromPrice, s => s.GetStartingPrice());

            TypeAdapterConfig<ItineraryDay, ItineraryDayReturnedDto>.NewConfig()
                .Map(d => d.Image, s => UrlHelper.Prefix(s.Image));

            TypeAdapterConfig<ItineraryEvent, ItineraryEventReturnedDto>.NewConfig();

            TypeAdapterConfig<Destination, DestinationReturnedDto>.NewConfig()
                .Map(d => d.Images, s => UrlHelper.PrefixAll(s.Images))
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage))
                .Map(d => d.StartingFromPrice, s => s.GetStartingPrice());

            TypeAdapterConfig<Destination, DestinationSummaryDto>.NewConfig()
                .Map(d => d.StartingFromPrice, s => s.GetStartingPrice());

            TypeAdapterConfig<Wedding, WeddingReturnedDto>.NewConfig()
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage));

            TypeAdapterConfig<Event, EventReturnedDto>.NewConfig()
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage));

            TypeAdapterConfig<Activity, ActivityReturnedDto>.NewConfig()
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage));

            TypeAdapterConfig<Service, ServiceReturnedDto>.NewConfig()
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage));

            TypeAdapterConfig<AboutUs, AboutUsReturnedDto>.NewConfig();

            TypeAdapterConfig<Founder, FounderReturnedDto>.NewConfig()
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage));

            TypeAdapterConfig<Blog, BlogReturnedDto>.NewConfig()
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage));

            TypeAdapterConfig<BlogSub, BlogSubReturnedDto>.NewConfig()
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage));

            TypeAdapterConfig<PaymentMethod, PaymentMethodReturnedDto>.NewConfig();

            TypeAdapterConfig<Review, ReviewReturnedDto>.NewConfig()
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage));

            TypeAdapterConfig<ReviewPlatform, ReviewPlatformReturnedDto>.NewConfig()
                .Map(d => d.Icon, s => UrlHelper.Prefix(s.Icon));

            TypeAdapterConfig<Reel, ReelReturnedDto>.NewConfig()
                .Map(d => d.CoverImage, s => UrlHelper.Prefix(s.CoverImage));

            TypeAdapterConfig<CustomerPhoto, CustomerPhotoReturnedDto>.NewConfig()
                .Map(d => d.Image, s => UrlHelper.Prefix(s.Image));

            TypeAdapterConfig<ReviewsSettings, ReviewsSettingsReturnedDto>.NewConfig()
                .Map(d => d.Image, s => UrlHelper.Prefix(s.Image));

            TypeAdapterConfig<SocialMediaLink, SocialMediaLinkReturnedDto>.NewConfig()
                .Map(d => d.Icon, s => UrlHelper.Prefix(s.Icon));

            TypeAdapterConfig<AuditLog, AuditLogReturnedDto>.NewConfig();
        }
    }
}
