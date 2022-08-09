using AccountsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsServiceTests
{
    internal class SampleData
    {
        internal const string SamplePassword = "ThisIsATestPass123";

        public static User GetSampleUser(bool isDeleted)
        {
            return new()
            {
                UserName = "SampleUserName",
                Email = "SampleUserEmail@mail.com",
                IsDeleted = isDeleted,
            };
        }
    }
}
