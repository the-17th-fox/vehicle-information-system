namespace AccountsService.Constants.Logger
{
    public class LoggingForms
    {
        public const string Registred = "User {username}:{email} has successfully registred.";
        public const string AddedToRole = "User {username}:{email} has successfully added to role {role}";
        public const string ExceptionForm = "{exception} has occured with status code {statusCode}: {message}";
    }
}
