namespace AccountsService.Exceptions.CustomExceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Not enough rights or invalid credentials") : base(message)
        {
        }
    }
}
