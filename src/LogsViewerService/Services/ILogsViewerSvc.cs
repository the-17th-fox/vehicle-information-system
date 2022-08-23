using LogsViewerService.Models;
using Common.Utilities.Pagination;
using Common.ViewModels;
using LogsViewerService.ViewModels;

namespace LogsViewerService.Services
{
    public interface ILogsViewerSvc
    {
        public Task<PagedList<LoggingRecord>> GetAllAsync(LogsParametersViewModel logsParams, PageParametersViewModel pageParams);
    }
}
