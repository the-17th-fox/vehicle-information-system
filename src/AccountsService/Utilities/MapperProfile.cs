using AccountsService.Models;
using AccountsService.Services.Pagination;
using AccountsService.ViewModels;
using AutoMapper;

namespace AccountsService.Utilities
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<RegistrationViewModel, User>();
            CreateMap<User, UserViewModel>();
            CreateMap<PagedList<User>, PageViewModel<UserViewModel>>()
                .ForMember(i => i.Items, p => p.MapFrom(u => u.ToList()));
        }
    }
}
