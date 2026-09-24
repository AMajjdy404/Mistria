using Mapster;
using GateOfEgypt.API.Helpers;
using GateOfEgypt.Domain.Interfaces;
using GateOfEgypt.Infrastructure.Implementation;
using System.Text.Json.Serialization;

namespace GateOfEgypt.API.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection Services, IConfiguration configuration)
        {

            UrlHelper.BaseApiUrl = configuration["BaseApiUrl"] ?? string.Empty;

            Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            Services.AddMapster();
            MappingConfig.Configure();
            Services.AddScoped<AdminSeeding>();
            Services.AddAuthorization();

            Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    });

            return Services;
        }
    }
}
