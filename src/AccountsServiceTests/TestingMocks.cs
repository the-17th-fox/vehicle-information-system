using AccountsService.Infrastructure.Context;
using AccountsService.Services;
using Common.Models.AccountsService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsServiceTests.Mocks
{
    internal class TestingMocks
    {
        internal Mock<UserManager<User>> UserManager { get; set; } = null!;
        internal Mock<ILogger<AccountsSvc>> Logger { get; set; } = null!;
        internal AccountsServiceContext Context { get; set; } = null!;
        internal RoleManager<IdentityRole<Guid>> RoleManager { get; set; } = null!;

        internal TestingMocks()
        {
            Logger = new();
            UserManager = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            RoleManager = new(Mock.Of<IRoleStore<IdentityRole<Guid>>>(), null, null, null, null);

            var options = new DbContextOptionsBuilder<AccountsServiceContext>()
                .UseInMemoryDatabase(databaseName: "AccountsDb")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            Context = new(options);
        }

        internal static AccountsSvc GetAccountsSvc(TestingMocks mocks) 
            => new AccountsSvc(mocks.UserManager.Object, mocks.Logger.Object, mocks.Context, mocks.RoleManager);
    }
}
