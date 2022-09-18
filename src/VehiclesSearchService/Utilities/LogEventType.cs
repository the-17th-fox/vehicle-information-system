using Common.Constants.Logger;

namespace VehiclesSearchService.Utilities
{
    public class LogEventType : CommonLoggingForms
    {
        public const string MfrsInfoRequest = "The user is trying to request detailed mfrs info. Params: [{params}]";
        public const string MfrsInfoRequestSucceded = "Mfrs info request was succeded. Params: [{params}]";
        public const string MfrsInfoRequestFailed = "Mfrs info request was failed. Params: [{params}]";

        public const string CacheRequested = "Trying to get cached data. Cache key name: [{cacheKeyName}]";
        public const string CacheNotFound = "Cache wasn't found. Cache key name: [{cacheKeyName}]";
        public const string CacheRetrieved = "Cache was retrieved successfully. Cache key name: [{cacheKeyName}]";
        public const string CacheUpdated = "Cache was updated successfully. Cache key name: [{cacheKeyName}]";

    }
}
