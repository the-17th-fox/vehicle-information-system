namespace Common.Constants.Logger
{
    public class CommonLoggingForms
    {
        public const string ExceptionForm = "[{exception}] has occured with status code [{statusCode}]: [{message}]";
        public const string ParameterMissed = "One or more required parameters are missing: null, empty, etc. Params: [{argument}]";
    }
}
