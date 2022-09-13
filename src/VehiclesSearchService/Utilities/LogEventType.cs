using Common.Constants.Logger;

namespace VehiclesSearchService.Utilities
{
    public class LogEventType : CommonLoggingForms
    {
        public const string MfrsInfoRequest = "The user is trying to request detailed mfrs info. Params: [{params}]";
        public const string MfrsInfoRequestSucceded = "Mfrs info request was succeded. Params: [{params}]";
        public const string MfrsInfoRequestFailed = "Mfrs info request was failed. Params: [{params}]";
    }
}
