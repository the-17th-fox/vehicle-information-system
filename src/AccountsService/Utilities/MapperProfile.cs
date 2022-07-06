using AccountsService.Models;
using AccountsService.ViewModels;
using AutoMapper;

namespace AccountsService.Utilities
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<RegistrationViewModel, User>();
        }
    }
}
