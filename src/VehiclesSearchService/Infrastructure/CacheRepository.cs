using StackExchange.Redis;

namespace VehiclesSearchService.Infrastructure
{
    public class CacheRepository : ICacheRepository
    {
        private readonly IDatabase _redis;
        public CacheRepository(IConnectionMultiplexer multiplexer)
        {
            _redis = multiplexer.GetDatabase();
        }

        public async Task<string> GetAsync(string keyName)
        {
            string? response = await _redis.StringGetAsync(keyName);
            return response is null ? string.Empty : response;
        }

        public async Task UpdateCacheAsync(string keyName, string responseJson, double expireAfter)
        {
            var setTask = _redis.StringSetAsync(keyName, responseJson);
            var expTask = _redis.KeyExpireAsync(keyName, DateTime.UtcNow.AddHours(expireAfter));
            await Task.WhenAll(setTask, expTask);
        }
    }
}
