﻿using Microsoft.AspNetCore.Identity;

namespace Common.Constants.Auth
{
    public class AccountsRoles
    {
        public const string DefaultUser = "DefaultUser";
        public const string Administrator = "Administrator";

        public static List<IdentityRole<Guid>> Roles { get; } = new()
        {
            new IdentityRole<Guid>(AccountsRoles.DefaultUser) { Id = Guid.NewGuid(), NormalizedName = AccountsRoles.DefaultUser.Normalize() },
            new IdentityRole<Guid>(AccountsRoles.Administrator) { Id = Guid.NewGuid(), NormalizedName = AccountsRoles.Administrator.Normalize() }
        };
    }
}
