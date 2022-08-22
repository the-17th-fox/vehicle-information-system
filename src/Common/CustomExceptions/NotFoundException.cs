namespace Common.CustomExceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message = "Requested data wasn't found") : base(message)
        { }
    }
}
