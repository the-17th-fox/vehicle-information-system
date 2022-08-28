﻿using AccountsService.ViewModels;
using AutoMapper;
using Common.Models.AccountsService;
using Common.Utilities;
using Common.ViewModels;

namespace AccountsService.Utilities
{
    internal class MapperProfile : Profile
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
