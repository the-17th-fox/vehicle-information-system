namespace VehiclesSearchService.Infrastructure
{
    public interface ICacheRepository
    {
        public Task<string> GetAsync(string cacheKeyName);
        public Task<TaskStatus> UpdateCacheAsync(string cacheKeyName, string responseJson, double expireAfter);
    }
}
