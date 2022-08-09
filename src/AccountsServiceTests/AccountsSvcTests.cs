using AccountsService.Exceptions.CustomExceptions;
using AccountsService.Infrastructure.Context;
using AccountsService.Models;
using AccountsService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using Xunit;

namespace AccountsServiceTests
{
    public class AccountsSvcTests
    {
        private static Mock<UserManager<User>> _userManagerMock = null!;
        private static Mock<ILogger<AccountsSvc>> _loggerMock = null!;
        private static AccountsServiceContext _context = null!;
        public AccountsSvcTests()
        {
            _loggerMock = new();
            _userManagerMock = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            var options = new DbContextOptionsBuilder<AccountsServiceContext>()
                .UseInMemoryDatabase(databaseName: "AccountsDb")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new(options);
        }

        [Fact]
        public async void Register_UserAlreadyExistsNotDeleted_InvalidParamsException()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: false);

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            var service = new AccountsSvc(_userManagerMock.Object, _loggerMock.Object, _context);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidParamsException>(() => service.RegisterAsync(user, SampleData.SamplePassword))
                .WaitAsync(CancellationToken.None);
            _userManagerMock.VerifyAll();
        }

        [Fact]
        public void Register_UserAlreadyExistsDeleted_AccountRestored()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: true);

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            var service = new AccountsSvc(_userManagerMock.Object, _loggerMock.Object, _context);

            // Act
            var result = service.RegisterAsync(user, SampleData.SamplePassword);

            // Assert
            _userManagerMock.VerifyAll();
            Assert.True(result.IsCompletedSuccessfully);
            Assert.False(user.IsDeleted);
        }

        [Fact]
        public void Register_UserNotExists_AccountCreated()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: false);
            User nullUser = null!;

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(nullUser)
                .Verifiable();

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            var service = new AccountsSvc(_userManagerMock.Object, _loggerMock.Object, _context);

            // Act 
            var result = service.RegisterAsync(user, SampleData.SamplePassword)
                .WaitAsync(CancellationToken.None);

            // Assert
            _userManagerMock.VerifyAll();
            Assert.True(result.IsCompletedSuccessfully);
        }
    }
}