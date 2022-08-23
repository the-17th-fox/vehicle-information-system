using Common.Constants.Logger;

namespace LogsViewerService.Constants.Logger
{
    internal class LogEventType : CommonLoggingForms
    {
        public const string LogsRetrievingAttempt = "The user [{username}]:[{email}] is trying to retrieve logs from the database";
        public const string LogsRetrieved = "The user [{username}]:[{email}] has successfully retrieved logs from the database";
        public const string DbConnectionEstablished = "Connection to the database [{params}] has been successfully established";
    }
}
