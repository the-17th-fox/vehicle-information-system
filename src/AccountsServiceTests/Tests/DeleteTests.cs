using AccountsService.Services;
using AccountsServiceTests.Mocks;
using AccountsServiceTests.TestingData;
using Common.CustomExceptions;
using Common.Models.AccountsService;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AccountsServiceTests.Tests
{
    public class DeleteTests
    {
        private readonly TestingMocks _mocks = new();

        [Fact]
        public async void Delete_UserNotExists_NotFoundException()
        {
            // Arrange
            User user = null!;

            _mocks.UserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            var service = TestingMocks.GetAccountsSvc(_mocks);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(It.IsAny<Guid>()))
                .WaitAsync(CancellationToken.None);
            _mocks.UserManager.VerifyAll();
        }

        [Fact]
        public async void Delete_UserHasAlreadyBeenDeleted_Exception()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: true);

            _mocks.UserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            var service = TestingMocks.GetAccountsSvc(_mocks);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.DeleteAsync(It.IsAny<Guid>()))
                .WaitAsync(CancellationToken.None);
            _mocks.UserManager.VerifyAll();
        }

        [Fact]
        public void Delete_UserExistsAndNotDeleted_Succeded()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: false);
            var loginInfos = SampleData.GetSampleUserLoginInfos();

            _mocks.UserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            _mocks.UserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(SampleData.GetSampleUserLoginInfos())
                .Verifiable();

            _mocks.UserManager.Setup(x => x.RemoveLoginAsync(user, loginInfos.FirstOrDefault()!.LoginProvider, loginInfos.FirstOrDefault()!.ProviderKey))
                .Verifiable();

            _mocks.UserManager.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            var service = TestingMocks.GetAccountsSvc(_mocks);

            // Act
            var result = service.DeleteAsync(It.IsAny<Guid>());

            // Assert
            _mocks.UserManager.VerifyAll();
            Assert.True(result.IsCompletedSuccessfully);
            Assert.True(user.IsDeleted);
        }
    }
}
