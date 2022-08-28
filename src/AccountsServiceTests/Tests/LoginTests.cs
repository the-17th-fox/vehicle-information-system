using Common.CustomExceptions;
using Common.Models.AccountsService;
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
    public class LoginTests
    {
        private readonly TestingMocks _mocks = new();

        [Fact]
        public async void Login_UserNotExists_NotFoundException()
        {
            // Arrange
            User user = null!;

            _mocks.UserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            var service = TestingMocks.GetAccountsSvc(_mocks);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), null!))
                .WaitAsync(CancellationToken.None);
            _mocks.UserManager.VerifyAll();
        }

        [Fact]
        public async void Login_UserIsDeleted_NotFoundException()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: true);

            _mocks.UserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            var service = TestingMocks.GetAccountsSvc(_mocks);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), null!))
                .WaitAsync(CancellationToken.None);
            _mocks.UserManager.VerifyAll();
        }

        [Fact]
        public async void Login_WrongPassword_UnauthorizedException()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: false);

            _mocks.UserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            _mocks.UserManager.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(false)
                .Verifiable();

            var service = TestingMocks.GetAccountsSvc(_mocks);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), SampleData.JwtConfig))
                .WaitAsync(CancellationToken.None);
            _mocks.UserManager.VerifyAll();
        }

        [Fact]
        public async void Login_LoginSucceded_StringWithJWT()
        {
            // Arrange
            var user = SampleData.GetSampleUser(isDeleted: false);

            _mocks.UserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            _mocks.UserManager.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(true)
                .Verifiable();

            _mocks.UserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(SampleData.GetSampleDefaultUserRoles);

            var service = TestingMocks.GetAccountsSvc(_mocks);

            // Act
            var result = await service.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), SampleData.JwtConfig);

            // Assert
            _mocks.UserManager.VerifyAll();
            Assert.False(string.IsNullOrWhiteSpace(result));
        }
    }
}
