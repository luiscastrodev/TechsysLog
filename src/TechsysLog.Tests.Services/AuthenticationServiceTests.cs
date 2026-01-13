using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Entities.Enums;
using TechsysLog.Domain.Interfaces;
using Xunit;

namespace TechsysLog.Tests.Services
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IPasswordHasherService> _mockPasswordHasher;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepo;
        private readonly AuthenticationService _sut;

        public AuthenticationServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasherService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockRefreshTokenRepo = new Mock<IRefreshTokenRepository>();
            _sut = new AuthenticationService(_mockUserRepo.Object, _mockPasswordHasher.Object,
                _mockTokenService.Object, _mockRefreshTokenRepo.Object);
        }

        [Fact]
        public async Task LoginAsyncWhenUserDoesntExistShouldThrowBusinessException()
        {
            var email = "ghost@example.com";
            var password = "senha123";

            _mockUserRepo.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((User?)null);

            var exception = await Assert.ThrowsAsync<BusinessException>(
                () => _sut.LoginAsync(email, password));

            Assert.Equal("Login ou senha inválidos.", exception.Message);
        }

        [Fact]
        public async Task LoginAsyncWhenAccountIsLockedShouldThrowException()
        {
            var email = "locked@example.com";
            var password = "senha123";
            var lockoutEnd = DateTime.UtcNow.AddHours(1);

            var user = new User("João", email, "hashedpass", UserRole.User)
            {
                LockoutEnd = lockoutEnd
            };

            _mockUserRepo.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            var exception = await Assert.ThrowsAsync<BusinessException>(
                () => _sut.LoginAsync(email, password));

            Assert.Contains("Conta bloqueada até", exception.Message);
        }

        [Fact]
        public async Task LoginAsyncWhenPasswordIsWrongShouldRegisterFailedAttempt()
        {
            var email = "user@example.com";
            var password = "wrongpass";
            var user = new User("Maria", email, "hashedpass", UserRole.User);

            _mockUserRepo.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _mockPasswordHasher.Setup(x => x.VerifyPassword(password, user.PasswordHash))
                .Returns(false);

            await Assert.ThrowsAsync<BusinessException>(
                () => _sut.LoginAsync(email, password));

            _mockUserRepo.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsyncWithValidCredentialsShouldReturnTokens()
        {
            var email = "valid@example.com";
            var password = "senha123";
            var userId = Guid.NewGuid();
            var user = new User("Pedro", email, "hashedpass", UserRole.Admin) { Id = userId };

            var tokenData = new AuthenticationResponseDTO
            {
                UserId = userId,
                AccessToken = "access_token_xyz",
                RefreshToken = "refresh_token_xyz",
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _mockUserRepo.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _mockPasswordHasher.Setup(x => x.VerifyPassword(password, user.PasswordHash))
                .Returns(true);

            _mockTokenService.Setup(x => x.GenerateToken(user))
                .Returns(tokenData);

            _mockRefreshTokenRepo.Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            _mockUserRepo.Setup(x => x.UpdateAsync(user))
                .Returns(Task.CompletedTask);

            var result = await _sut.LoginAsync(email, password);

            Assert.NotNull(result.Data);
            Assert.Equal(tokenData.AccessToken, result.Data.AccessToken);
            _mockRefreshTokenRepo.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        [Fact]
        public async Task RegisterFailedLoginAsyncWhenMaxAttemptsReachedShouldLockAccount()
        {
            var user = new User("Teste", "teste@example.com", "hash", UserRole.User)
            {
                FailedLoginAttempts = 4
            };

            await _sut.RegisterFailedLoginAsync(user, maxAttempts: 5, TimeSpan.FromMinutes(15));

            Assert.Equal(0, user.FailedLoginAttempts);
            Assert.NotNull(user.LockoutEnd);
        }

        [Fact]
        public async Task UnlockUserAsyncShouldClearLockoutAndResetAttempts()
        {
            var userId = Guid.NewGuid();
            var user = new User("Bruno", "bruno@example.com", "hash", UserRole.User)
            {
                Id = userId,
                LockoutEnd = DateTime.UtcNow.AddHours(1),
                FailedLoginAttempts = 5
            };

            _mockUserRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockUserRepo.Setup(x => x.UpdateAsync(user))
                .Returns(Task.CompletedTask);

            await _sut.UnlockUserAsync(userId);

            Assert.Null(user.LockoutEnd);
            Assert.Equal(0, user.FailedLoginAttempts);
        }

        [Fact]
        public async Task RefreshTokenAsyncWhenTokenIsExpiredShouldReturnFailure()
        {
            var expiredToken = "expired_token_123";

            _mockRefreshTokenRepo.Setup(x => x.GetByTokenAsync(expiredToken))
                .ReturnsAsync((RefreshToken?)null);

            var result = await _sut.RefreshToken(expiredToken);

            Assert.False(result.IsSuccess);
            Assert.Equal("Token expirado ou invalido", result.Message);
        }

        [Fact]
        public async Task RefreshTokenAsyncWithValidTokenShouldGenerateNewAccessToken()
        {
            var userId = Guid.NewGuid();
            var refreshToken = "valid_refresh_token";
            var user = new User("Ana", "ana@example.com", "hash", UserRole.User) { Id = userId };

            var storedToken = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(3)
            };

            var newTokenData = new AuthenticationResponseDTO
            {
                UserId = userId,
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _mockRefreshTokenRepo.Setup(x => x.GetByTokenAsync(refreshToken))
                .ReturnsAsync(storedToken);

            _mockUserRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockTokenService.Setup(x => x.GenerateToken(user))
                .Returns(newTokenData);

            _mockRefreshTokenRepo.Setup(x => x.UpdateAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            _mockRefreshTokenRepo.Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.RefreshToken(refreshToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(newTokenData.AccessToken, result.Data.AccessToken);
        }
    }
}
