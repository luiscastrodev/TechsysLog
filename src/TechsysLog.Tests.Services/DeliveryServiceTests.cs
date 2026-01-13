using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Entities.ENUMS;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using Xunit;

namespace TechsysLog.Tests.Services
{
    public class DeliveryServiceTests
    {
        private readonly Mock<IDeliveryRepository> _mockDeliveryRepo;
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<INotificationRepository> _mockNotificationRepo;
        private readonly Mock<INotificationHubService> _mockNotificationHub;
        private readonly DeliveryService _sut;

        public DeliveryServiceTests()
        {
            _mockDeliveryRepo = new Mock<IDeliveryRepository>();
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockNotificationRepo = new Mock<INotificationRepository>();
            _mockNotificationHub = new Mock<INotificationHubService>();
            _sut = new DeliveryService(_mockDeliveryRepo.Object, _mockOrderRepo.Object,
                _mockNotificationRepo.Object, _mockNotificationHub.Object);
        }

        [Fact]
        public async Task RegisterDeliveryAsyncWhenOrderNotFoundShouldThrow()
        {
            var dto = new DeliveryDto("TECHSYS-999", DateTime.Now, "Fulano", "Nenhuma");

            _mockOrderRepo.Setup(x => x.GetByOrderNumberAsync(dto.OrderNumber))
                .ReturnsAsync((Order?)null);

            await Assert.ThrowsAsync<Exception>(
                () => _sut.RegisterDeliveryAsync(dto));
        }

        [Fact]
        public async Task RegisterDeliveryAsyncShouldMarkOrderAsDeliveredAndNotifyCustomer()
        {
            var userId = Guid.NewGuid();
            var orderNumber = "TECHSYS-777";
            var receivedBy = "Marisa da Silva";

            var order = new Order(orderNumber, userId, "Pacote", 50m,
                new Address { City = "Brasília" }, "Marisa")
            { Id = Guid.NewGuid() };

            var dto = new DeliveryDto(orderNumber, DateTime.Now, receivedBy, "Entregar na portaria");

            _mockOrderRepo.Setup(x => x.GetByOrderNumberAsync(orderNumber))
                .ReturnsAsync(order);

            _mockOrderRepo.Setup(x => x.UpdateAsync(order))
                .Returns(Task.CompletedTask);

            _mockDeliveryRepo.Setup(x => x.AddAsync(It.IsAny<Delivery>()))
                .Returns(Task.CompletedTask);

            _mockNotificationRepo.Setup(x => x.AddAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockNotificationHub.Setup(x => x.NotifyOrderDeliveryAsync(userId, orderNumber, receivedBy))
                .Returns(Task.CompletedTask);

            var result = await _sut.RegisterDeliveryAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(OrderStatus.Delivered, order.Status);
            _mockNotificationHub.Verify(x => x.NotifyOrderDeliveryAsync(userId, orderNumber, receivedBy), Times.Once);
        }

        [Fact]
        public async Task RegisterDeliveryAsyncWithNotesShouldIncludeNotesInDelivery()
        {
            var userId = Guid.NewGuid();
            var notes = "Cliente ausente, deixado com vizinho";
            var order = new Order("TECHSYS-888", userId, "Caixa", 75m,
                new Address { City = "Curitiba" }, "Roberto")
            { Id = Guid.NewGuid() };

            var dto = new DeliveryDto("TECHSYS-888", DateTime.Now, "Vizinho do Lado", notes);

            _mockOrderRepo.Setup(x => x.GetByOrderNumberAsync(dto.OrderNumber))
                .ReturnsAsync(order);

            _mockOrderRepo.Setup(x => x.UpdateAsync(order))
                .Returns(Task.CompletedTask);

            _mockDeliveryRepo.Setup(x => x.AddAsync(It.IsAny<Delivery>()))
                .Returns(Task.CompletedTask);

            _mockNotificationRepo.Setup(x => x.AddAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockNotificationHub.Setup(x => x.NotifyOrderDeliveryAsync(It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.RegisterDeliveryAsync(dto);

            Assert.True(result.IsSuccess);
            _mockDeliveryRepo.Verify(x => x.AddAsync(It.Is<Delivery>(d => d.Notes == notes)), Times.Once);
        }
    }
}
