using LogsViewerService.Infrastructure;
using Common.CustomExceptions;
using Common.Utilities.Pagination;
using Common.ViewModels;
using LogsViewerService.Constants.Logger;
using LogsViewerService.Infrastructure.Context;
using LogsViewerService.Models;
using LogsViewerService.ViewModels;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LogsViewerService.Services
{
    public class LogsViewerSvc : ILogsViewerSvc
    {
        private readonly ILogger<LogsViewerSvc> _logger;
        private readonly IOptions<LogsContextConfiguration> _mongoConfig;

        public LogsViewerSvc(
            ILogger<LogsViewerSvc> logger, 
            IOptions<LogsContextConfiguration> mongoConfig)
        {
            _logger = logger;
            _mongoConfig = mongoConfig;
        }

        public async Task<PagedList<LoggingRecord>> GetAllAsync(LogsParametersViewModel logsParams, PageParametersViewModel pageParams)
        {
            if (string.IsNullOrWhiteSpace(_mongoConfig.Value.ConnectionString))
            {
                var argument = nameof(_mongoConfig.Value.ConnectionString);
                _logger.LogError(LogEventType.ParameterMissed, argument);
                throw new InvalidParamsException(LogEventType.ParameterMissed.Replace("{argument}", argument));
            }

            if (string.IsNullOrWhiteSpace(_mongoConfig.Value.CollectionName))
            {
                var argument = nameof(_mongoConfig.Value.CollectionName);
                _logger.LogError(LogEventType.ParameterMissed, argument);
                throw new InvalidParamsException(LogEventType.ParameterMissed.Replace("{argument}", argument));
            }

            var context = new MongoDbContext<LoggingRecord>(_mongoConfig.Value.ConnectionString, _mongoConfig.Value.CollectionName);

            _logger.LogInformation(LogEventType.DbConnectionEstablished, context.DatabaseName);

            var logsColl = context.GetCollection();

            var fromDate = new DateTime(
                year: logsParams.FromYear,
                month: logsParams.FromMonth,
                day: logsParams.FromDay);

            var logLevelFilter = Builders<LoggingRecord>.Filter
                .Eq(nameof(LoggingRecord.LogLevel), Enum.GetName<LogLevel>(logsParams.LowestLoggingLevel));

            var dateFilter = Builders<LoggingRecord>.Filter
                .Gte(nameof(LoggingRecord.UtcTimestamp), $"{fromDate:u}");

            var sort = Builders<LoggingRecord>.Sort.Descending(nameof(LoggingRecord.UtcTimestamp));

            var logs = logsColl
                .Find(Builders<LoggingRecord>.Filter.And(logLevelFilter, dateFilter))
                .Sort(sort);

            return await PagedList<LoggingRecord>.ToPagedListAsync(logs, pageParams.PageNumber, pageParams.PageSize);
        }
    }
}
