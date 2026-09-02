using AutoMapper;
using Mistria.API.Dtos;
using Mistria.Domain.Models;

namespace Mistria.API.Helpers
{
    public class CustomerPhotoImageUrlResolver : IValueResolver<CustomerPhoto, CustomerPhotoReturnedDto, string>
    {
        private readonly IConfiguration _configuration;

        public CustomerPhotoImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(CustomerPhoto source, CustomerPhotoReturnedDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.Image))
                return $"{_configuration["BaseApiUrl"]}{source.Image}";
            return string.Empty;
        }
    }
}
