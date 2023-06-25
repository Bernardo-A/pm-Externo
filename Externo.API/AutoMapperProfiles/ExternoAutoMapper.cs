using Externo.API.ViewModels;
using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Externo.API.AutoMapperProfiles
{
    public class ExternoAutoMapperProfile : Profile
    {
        public ExternoAutoMapperProfile()
        {
            CreateMap<EmailInsertViewModel, EmailViewModel>();
        }
    }
}