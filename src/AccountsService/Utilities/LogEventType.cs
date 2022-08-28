using Common.Constants.Logger;

namespace AccountsService.Utilities
{
    internal class LogEventType : CommonLoggingForms
    {
        public const string RegistrationAttempt = "The user [{username}]:[{email}] is trying to register";
        public const string Registered = "The user [{username}]:[{email}] was successfully registred";
        public const string FailedToRegister = "The user [{username}]:[{email}] has failed the registration: [{error}]";

        public const string LoginAttempt = "The user [{email}] is trying to log in";
        public const string LoggedIn = "The user [{email}] was successfully logged in";

        public const string AddedToRole = "The user [{id}] was successfully added to role [{role}]";
        public const string FailedToAddToRole = "The error occured while adding user [{id}] to role {role} [{error}]";
        public const string RoleNotFound = "Role [{role}] not found";
        public const string UserAlreadyInRole = "This user [{userId}] already in role [{role}]";
        public const string Restored = "The user account [{identifier}] has been successfully restored";

        public const string DeletionAttempt = "There was attempt to delete user [{id}]";
        public const string UserDeleted = "The user [{id}] was successfully deleted";
        public const string FailedToDelete = "The error occured during user [{id}] deletion: [{error}]";
        public const string AlreadyDeleted = "The user [{id}] is already deleted";

        public const string UserNotFound = "The user [{identifier}] was not found";
        public const string UserAlreadyExists = "The user [{identifier}] is already exists";
        public const string InvalidCredentials = "The user [{email}] has failed authentication";

        public const string TryingToGetUsers = "Trying to get users";
        public const string GotUsers = "Got a list of users";
        public const string NoUsersFound = "No users were found";

        public const string GoogleAuthPassed = "The user [{GoogleID}]:[{email}] has successfully passed the Google external authentication";
        public const string GoogleLoggedIn = "The user [{GoogleID}]:[{email}] was successfully logged in through the Google";
        public const string GoogleLogout = "The user (google) [{id}]:[{email}] has successfully logged out";
    }
}
