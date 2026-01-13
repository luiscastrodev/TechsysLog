using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Entities.Enums;
using TechsysLog.Domain.Entities.ENUMS;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using Xunit;

namespace TechsysLog.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<ICepService> _mockCepService;
        private readonly Mock<INotificationRepository> _mockNotificationRepo;
        private readonly Mock<IOrderHistoryRepository> _mockOrderHistoryRepo;
        private readonly Mock<INotificationHubService> _mockNotificationHub;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockCepService = new Mock<ICepService>();
            _mockNotificationRepo = new Mock<INotificationRepository>();
            _mockOrderHistoryRepo = new Mock<IOrderHistoryRepository>();
            _mockNotificationHub = new Mock<INotificationHubService>();
            _mockUserRepo = new Mock<IUserRepository>();
            _sut = new OrderService(_mockOrderRepo.Object, _mockCepService.Object,
                _mockNotificationRepo.Object, _mockOrderHistoryRepo.Object,
                _mockNotificationHub.Object, _mockUserRepo.Object);
        }

        [Fact]
        public async Task CreateOrderAsyncWhenAddressNotFoundShouldThrow()
        {
            var userId = Guid.NewGuid();
            var dto = new CreateOrderDto(
                Description: "Produto X",
                Amount: 99.99m,
                Address: new AddressDto("12345-678", "123"),
                UserId: null
            );

            _mockCepService.Setup(x => x.GetAddressByCepAsync(dto.Address.ZipCode))
                .ReturnsAsync((Address?)null);

            await Assert.ThrowsAsync<Exception>(
                () => _sut.CreateOrderAsync(userId, dto));
        }

        [Fact]
        public async Task CreateOrderAsyncShouldCreateNotificationAndBroadcastViaSignalR()
        {
            var userId = Guid.NewGuid();
            var userName = "Carlos Silva";
            var user = new User(userName, "carlos@example.com", "hash", UserRole.User) { Id = userId };

            var dto = new CreateOrderDto(
                Description: "Produto Super Top",
                Amount: 199.99m,
                Address: new AddressDto("01310-100", "1000"),
                UserId: null
            );

            var address = new Address
            {
                ZipCode = "01310-100",
                Street = "Avenida Paulista",
                Neighborhood = "Bela Vista",
                City = "São Paulo",
                State = "SP"
            };

            _mockCepService.Setup(x => x.GetAddressByCepAsync(dto.Address.ZipCode))
                .ReturnsAsync(address);

            _mockUserRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockOrderRepo.Setup(x => x.AddAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            _mockNotificationRepo.Setup(x => x.AddAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockNotificationHub.Setup(x => x.NotifyNewOrderAsync(userId, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.CreateOrderAsync(userId, dto);

            Assert.NotNull(result.Data);
            _mockNotificationRepo.Verify(x => x.AddAsync(It.IsAny<Notification>()), Times.Once);
            _mockNotificationHub.Verify(x => x.NotifyNewOrderAsync(userId, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetUserOrdersAsyncShouldReturnOrdersWithHistory()
        {
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var order = new Order("TECHSYS-001", userId, "Produto", 100m,
                new Address { City = "São Paulo", State = "SP" }, "João");

            var history = new List<OrderHistory>
            {
                new OrderHistory(orderId, OrderStatus.Pending, OrderStatus.Processing, userId, "Iniciado")
            };

            _mockOrderRepo.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(new List<Order> { order });

            _mockOrderHistoryRepo.Setup(x => x.GetByOrderIdAsync(orderId))
                .ReturnsAsync(history);

            var result = await _sut.GetUserOrdersAsync(userId);

            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task GetByNumberAsyncWhenOrderNotFoundShouldReturnNull()
        {
            var orderNumber = "TECHSYS-999";

            _mockOrderRepo.Setup(x => x.GetByOrderNumberAsync(orderNumber))
                .ReturnsAsync((Order?)null);

            var result = await _sut.GetByNumberAsync(orderNumber);

            Assert.Null(result.Data);
        }

        [Fact]
        public async Task ChangeOrderStatusAsyncShouldCreateHistoryAndNotifyUser()
        {
            var userId = Guid.NewGuid();
            var changedByUserId = Guid.NewGuid();
            var order = new Order("TECHSYS-555", userId, "Produto", 150m,
                new Address { City = "Rio" }, "Fernanda")
            { Id = Guid.NewGuid() };

            _mockOrderRepo.Setup(x => x.GetByOrderNumberAsync(order.OrderNumber))
                .ReturnsAsync(order);

            _mockOrderRepo.Setup(x => x.UpdateAsync(order))
                .Returns(Task.CompletedTask);

            _mockOrderHistoryRepo.Setup(x => x.AddAsync(It.IsAny<OrderHistory>()))
                .Returns(Task.CompletedTask);

            _mockNotificationRepo.Setup(x => x.AddAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockNotificationHub.Setup(x => x.NotifyOrderStatusChangeAsync(It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.ChangeOrderStatusAsync(
                order.OrderNumber, (int)OrderStatus.Processing, changedByUserId, "Começou a processar");

            Assert.NotNull(result.Data);
            _mockOrderHistoryRepo.Verify(x => x.AddAsync(It.IsAny<OrderHistory>()), Times.Once);
            _mockNotificationHub.Verify(x => x.NotifyOrderStatusChangeAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
