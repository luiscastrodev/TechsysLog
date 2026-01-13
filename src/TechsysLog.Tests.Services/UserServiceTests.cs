using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Entities.Enums;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using Xunit;

namespace TechsysLog.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _sut = new UserService(_mockUserRepo.Object);
        }

        [Fact]
        public async Task RegisterAsyncWhenEmailAlreadyExistsShouldThrow()
        {
            var dto = new CreateUserDto
            {
                Name = "Novo User",
                Email = "existing@example.com",
                Password = "senha123",
                Role = UserRole.User
            };

            _mockUserRepo.Setup(x => x.EmailExistsAsync(dto.Email))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<BusinessException>(
                () => _sut.RegisterAsync(dto));
        }

        [Fact]
        public async Task RegisterAsyncWithNewEmailShouldCreateUser()
        {
            var dto = new CreateUserDto
            {
                Name = "Beatriz Costa",
                Email = "beatriz@example.com",
                Password = "senha123",
                Role = UserRole.User
            };

            _mockUserRepo.Setup(x => x.EmailExistsAsync(dto.Email))
                .ReturnsAsync(false);

            _mockUserRepo.Setup(x => x.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.RegisterAsync(dto);

            Assert.NotNull(result.Data);
            Assert.Equal(dto.Name, result.Data.Name);
            Assert.Equal(dto.Email, result.Data.Email);
        }

        [Fact]
        public async Task GetByIdAsyncWhenUserExistsShouldReturnUser()
        {
            var userId = Guid.NewGuid();
            var user = new User("Gustavo", "gustavo@example.com", "hash", UserRole.Admin) { Id = userId };

            _mockUserRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var result = await _sut.GetById(userId);

            Assert.NotNull(result.Data);
            Assert.Equal(user.Name, result.Data.Name);
        }

        [Fact]
        public async Task GetByIdAsyncWhenUserDoesntExistShouldReturnFailure()
        {
            var userId = Guid.NewGuid();

            _mockUserRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            var result = await _sut.GetById(userId);

            Assert.False(result.IsSuccess);
            Assert.Equal("Não encontrado.", result.Message);
        }

        [Fact]
        public async Task GetAllAsyncShouldReturnAllUsers()
        {
            var users = new List<User>
            {
                new User("Débora", "debora@example.com", "hash", UserRole.User),
                new User("Emerson", "emerson@example.com", "hash", UserRole.Admin),
                new User("Fabiana", "fabiana@example.com", "hash", UserRole.User)
            };

            _mockUserRepo.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);

            var result = await _sut.GetAllAsync();

            Assert.Equal(3, result.Data.Count());
        }
    }
}
