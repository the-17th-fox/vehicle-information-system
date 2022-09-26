using StackExchange.Redis;
using VehiclesSearchService.Utilities;

namespace VehiclesSearchService.Infrastructure
{
    public class CacheRepository : ICacheRepository
    {
        private readonly IDatabase _redis;
        private readonly ILogger<CacheRepository> _logger;
        public CacheRepository(IConnectionMultiplexer multiplexer, ILogger<CacheRepository> logger)
        {
            _redis = multiplexer.GetDatabase();
            _logger = logger;
        }

        public async Task<string> GetAsync(string keyName)
        {
            _logger.LogInformation(LogEventType.CacheRequested, keyName);
            if (!_redis.IsConnected(keyName))
            {
                _logger.LogError(LogEventType.CacheDbConnectionFailed);
                return string.Empty;
            }

            string? response = await _redis.StringGetAsync(keyName);
            return response is null ? string.Empty : response;
        }

        public async Task<TaskStatus> UpdateCacheAsync(string keyName, string responseJson, double expireAfter)
        {
            if (!_redis.IsConnected(keyName))
            {
                _logger.LogError(LogEventType.CacheDbConnectionFailed);
                return TaskStatus.Faulted;
            }

            var transaction = _redis.CreateTransaction(new());

            var setTask = transaction.StringSetAsync(keyName, responseJson);
            var expTask = transaction.KeyExpireAsync(keyName, DateTime.UtcNow.AddHours(expireAfter));

            await Task.WhenAll(transaction.ExecuteAsync());

            _logger.LogInformation(LogEventType.CacheUpdated, keyName);
            return TaskStatus.RanToCompletion;
        }
    }
}
