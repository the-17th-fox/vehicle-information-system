namespace AccountsService.Exceptions.CustomExceptions
{
    public class RegistrationFailedException : Exception
    {
        public RegistrationFailedException(string message = "An error occured during registration") : base(message)
        {
        }
    }
}
