using Common.ViewModels.ErrorMessages;
using System.ComponentModel.DataAnnotations;

namespace LogsViewerService.ViewModels
{
    /// <summary>
    /// Used for requesting a list of logs from the db using the params below
    /// </summary>
    public class LogsParametersViewModel
    {
        [Range(minimum: 1, maximum: 31, ErrorMessage = ViewModelsErrorMessages.OutOfRange)]
        public byte FromDay { get; set; } = 1;

        [Range(minimum: 1, maximum: 12, ErrorMessage = ViewModelsErrorMessages.OutOfRange)]
        public byte FromMonth { get; set; } = (byte)DateTime.UtcNow.Month;

        [Range(minimum: 2022, maximum: short.MaxValue, ErrorMessage = ViewModelsErrorMessages.OutOfRange)]
        public short FromYear { get; set; } = (short)DateTime.UtcNow.Year;

        [Range(minimum: 0, maximum: 6, ErrorMessage = ViewModelsErrorMessages.OutOfRange)]
        public LogLevel LowestLoggingLevel { get; set; } = LogLevel.Debug;
    }
}
