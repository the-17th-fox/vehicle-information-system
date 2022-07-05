namespace AccountsService.Auth
{
    /// <summary>
    /// Provides identity policies name. Should be used instead of direct role's-name specification
    /// </summary>
    public class AccountsPolicies
    {
        // Will provide access for all roles, such as admin, user, etc
        public const string DefaultRights = "DefaultRights";    

        // Will provide access for such roles as admin
        public const string ElevatedRights = "ElevatedRights";  
    }
}
