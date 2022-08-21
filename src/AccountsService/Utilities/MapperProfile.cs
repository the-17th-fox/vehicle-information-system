using AccountsService.ViewModels;
using AutoMapper;
using Common.Models.AccountsService;
using Common.Utilities.Pagination;
using Common.ViewModels;

namespace AccountsService.Utilities
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<RegistrationViewModel, User>();
            CreateMap<User, UserViewModel>();
            CreateMap<LoggingRecord, LoggingRecordViewModel>();
            CreateMap<PagedList<User>, PageViewModel<UserViewModel>>()
                .ForMember(i => i.Items, p => p.MapFrom(u => u.ToList()));
            CreateMap<PagedList<LoggingRecord>, PageViewModel<LoggingRecordViewModel>>()
                .ForMember(i => i.Items, p => p.MapFrom(u => u.ToList()));
        }
    }
}
