using Externo.API.ViewModels;
using AutoMapper;
 
namespace Externo.API.AutoMapperProfiles
{
    public class ExternoAutoMapperProfile : Profile {
        public ExternoAutoMapperProfile()
        {
            CreateMap<EmailInsertViewModel, EmailViewModel>().ForMember(dest => dest.Id, opt => opt.MapFrom(src => 0));
        }
    }
}