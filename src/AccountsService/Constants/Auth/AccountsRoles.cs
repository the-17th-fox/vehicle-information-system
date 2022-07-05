using Microsoft.AspNetCore.Identity;

namespace AccountsService.Constants.Auth
{
    public class AccountsRoles
    {
        public const string DefaultUser = "DefaultUser";
        public const string Administrator = "Administrator";

        private static readonly List<IdentityRole<Guid>> Roles = new()
        {
            new IdentityRole<Guid>(AccountsRoles.DefaultUser) { Id = Guid.NewGuid(), NormalizedName = AccountsRoles.DefaultUser.Normalize() },
            new IdentityRole<Guid>(AccountsRoles.Administrator) { Id = Guid.NewGuid(), NormalizedName = AccountsRoles.Administrator.Normalize() }
        };
        public static List<IdentityRole<Guid>> GetRoles() => Roles;
    }
}
