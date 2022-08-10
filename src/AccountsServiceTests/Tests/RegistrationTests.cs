using AccountsService.Exceptions.CustomExceptions;
using AccountsService.Infrastructure.Context;
using AccountsService.Models;
using AccountsService.Services;
using AccountsServiceTests.Mocks;
using AccountsServiceTests.TestingData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using Xunit;

namespace AccountsServiceTests.Tests
{
    public class RegistrationTests
    {
        private readonly TestingMocks _mocks = new();

        [Fact]
        public async void Register_UserAlreadyExistsNotDeleted_InvalidParamsException()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: false);

            _mocks.UserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            var service = new AccountsSvc(_mocks.UserManager.Object, _mocks.Logger.Object, _mocks.Context);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidParamsException>(() => service.RegisterAsync(user, SampleData.SamplePassword))
                .WaitAsync(CancellationToken.None);
            _mocks.UserManager.VerifyAll();
        }

        [Fact]
        public void Register_UserAlreadyExistsDeleted_AccountRestored()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: true);

            _mocks.UserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            var service = new AccountsSvc(_mocks.UserManager.Object, _mocks.Logger.Object, _mocks.Context);

            // Act
            var result = service.RegisterAsync(user, SampleData.SamplePassword);

            // Assert
            _mocks.UserManager.VerifyAll();
            Assert.True(result.IsCompletedSuccessfully);
            Assert.False(user.IsDeleted);
        }

        [Fact]
        public void Register_UserNotExists_AccountCreated()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: false);
            User nullUser = null!;

            _mocks.UserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(nullUser)
                .Verifiable();

            _mocks.UserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            _mocks.UserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            var service = new AccountsSvc(_mocks.UserManager.Object, _mocks.Logger.Object, _mocks.Context);

            // Act 
            var result = service.RegisterAsync(user, SampleData.SamplePassword)
                .WaitAsync(CancellationToken.None);

            // Assert
            _mocks.UserManager.VerifyAll();
            Assert.True(result.IsCompletedSuccessfully);
        }
    }
}