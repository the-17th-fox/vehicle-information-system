namespace LogsViewerService.ViewModels
{
    public class LoggingRecordViewModel
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string LogLevel { get; set; } = string.Empty;
        public string MessageTemplate { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Properties { get; set; }
        public string UtcTimestamp { get; set; } = string.Empty;
    }
}
