using AccountsService.Models;

namespace AccountsService.Utilities
{
    /// <summary>
    /// Is used to compose a message for logging
    /// </summary>
    public class LoggerHelper
    {
        public static string LogUserActions(string action, User user, params string[] msg)
        {
            string fullAction = action;
            if(msg.Count() > 0)
                fullAction += $"({string.Concat(msg)})";

            return $"{DateTime.UtcNow} | [{user.UserName}]:[{user.Email}] -- {fullAction}";
        }
    }
}
