using Common.Utilities;
using LogsViewerService.Models;
using Common.ViewModels;
using LogsViewerService.ViewModels;

namespace LogsViewerService.Services
{
    public interface ILogsViewerSvc
    {
        public Task<PagedList<LoggingRecord>> GetAllAsync(LogsParametersViewModel logsParams, PageParametersViewModel pageParams);
    }
}
