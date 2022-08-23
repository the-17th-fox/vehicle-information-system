using AutoMapper;
using Common.Utilities.Pagination;
using Common.ViewModels;
using LogsViewerService.Models;
using LogsViewerService.ViewModels;

namespace LogsViewerService.Utilities
{
    internal class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<LoggingRecord, LoggingRecordViewModel>();
            CreateMap<PagedList<LoggingRecord>, PageViewModel<LoggingRecordViewModel>>()
                .ForMember(i => i.Items, p => p.MapFrom(u => u.ToList()));
        }
    }
}
