namespace Common.CustomExceptions
{
    public class InvalidParamsException : Exception
    {
        public InvalidParamsException(string message = "Invalid data was provided") : base(message)
        {
        }
    }
}
