using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Entities.ENUMS;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using Xunit;

namespace TechsysLog.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _mockNotificationRepo;
        private readonly NotificationService _sut;

        public NotificationServiceTests()
        {
            _mockNotificationRepo = new Mock<INotificationRepository>();
            _sut = new NotificationService(_mockNotificationRepo.Object);
        }

        [Fact]
        public async Task GetUserNotificationsAsyncShouldReturnUserNotifications()
        {
            var userId = Guid.NewGuid();
            var notifications = new List<Notification>
            {
                new Notification(userId, "Pedido Criado", "Seu pedido foi criado!", NotificationType.Success, Guid.NewGuid(), "Order"),
                new Notification(userId, "Pedido Processando", "Seu pedido está sendo processado", NotificationType.Info, Guid.NewGuid(), "Order")
            };

            _mockNotificationRepo.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(notifications);

            var result = await _sut.GetUserNotificationsAsync(userId);

            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetUnreadCountAsyncShouldReturnCorrectCount()
        {
            var userId = Guid.NewGuid();

            _mockNotificationRepo.Setup(x => x.GetUnreadCountAsync(userId))
                .ReturnsAsync(5);

            var result = await _sut.GetUnreadCountAsync(userId);

            Assert.Equal(5, result.Data);
        }

        [Fact]
        public async Task MarkAsReadAsyncShouldMarkNotificationAsRead()
        {
            var notificationId = Guid.NewGuid();

            _mockNotificationRepo.Setup(x => x.MarkAsReadAsync(notificationId))
                .Returns(Task.CompletedTask);

            var result = await _sut.MarkAsReadAsync(notificationId);

            Assert.True(result.IsSuccess);
            _mockNotificationRepo.Verify(x => x.MarkAsReadAsync(notificationId), Times.Once);
        }

        [Fact]
        public async Task MarkAllAsReadAsyncShouldMarkAllUserNotificationsAsRead()
        {
            var userId = Guid.NewGuid();

            _mockNotificationRepo.Setup(x => x.MarkAllAsReadAsync(userId))
                .Returns(Task.CompletedTask);

            var result = await _sut.MarkAllAsReadAsync(userId);

            Assert.True(result.IsSuccess);
            _mockNotificationRepo.Verify(x => x.MarkAllAsReadAsync(userId), Times.Once);
        }
    }
}
