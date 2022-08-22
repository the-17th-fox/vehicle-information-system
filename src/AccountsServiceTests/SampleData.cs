using AccountsService.Utilities;
using Common.Constants.Auth;
using Common.Models.AccountsService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsServiceTests.TestingData
{
    internal class SampleData
    {
        internal const string SamplePassword = "ThisIsATestPass123";
        internal static readonly IOptions<JwtConfigurationModel> JwtConfig = GetSampleJwtOptions();

        public static User GetSampleUser(bool isDeleted)
        {
            return new()
            {
                UserName = "SampleUserName",
                Email = "SampleUserEmail@mail.com",
                IsDeleted = isDeleted,
            };
        }

        public static IOptions<JwtConfigurationModel> GetSampleJwtOptions()
        {
            return Options.Create(new JwtConfigurationModel()
            {
                Key = "ThisIsASampleJsonWebTokenSecurityKey",
                Audience = "SampleAudience",
                Issuer = "SampleIssuer",
                LifetimeHours = 1
            });
        }

        public static IList<UserLoginInfo> GetSampleUserLoginInfos()
        {
            return new List<UserLoginInfo>() { new UserLoginInfo("SampleLoginProvider", "SampleProviderKey", "SampleDisplayName") };
        }

        public static IList<string> GetSampleDefaultUserRoles()
        {
            return new List<string>() { AccountsRoles.DefaultUser };
        }
    }
}
