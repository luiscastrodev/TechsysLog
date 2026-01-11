using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Hubs;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.Application.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyUserAsync(Guid userId, NotificationDto notification)
        {
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
        }

        public async Task NotifyOrderStatusChangeAsync(Guid userId, string orderNumber, string previousStatus, string newStatus)
        {
            var notification = new
            {
                OrderNumber = orderNumber,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                Timestamp = DateTime.UtcNow,
                Message = $"O pedido {orderNumber} mudou de {previousStatus} para {newStatus}."
            };

            await _hubContext.Clients.User(userId.ToString()).SendAsync("OrderStatusChanged", notification);
        }

        public async Task NotifyOrderDeliveryAsync(Guid userId, string orderNumber, string userReceived)
        {
            var notification = new
            {
                OrderNumber = orderNumber,
                UserReceived = userReceived,
                Timestamp = DateTime.UtcNow,
                Message = $"O pedido {orderNumber} foi entregue para {userReceived}."
            };

            await _hubContext.Clients.User(userId.ToString()).SendAsync("OrderDelivered", notification);
        }

        public async Task NotifyNewOrderAsync(Guid userId, string orderNumber)
        {
            var notification = new
            {
                OrderNumber = orderNumber,
                Timestamp = DateTime.UtcNow,
                Message = $"Seu pedido {orderNumber} foi criado com sucesso!"
            };

            await _hubContext.Clients.User(userId.ToString()).SendAsync("NewOrderCreated", notification);
        }

        public async Task NotifyAllAsync(NotificationDto notification)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
        }

        public async Task NotifyGroupAsync(string groupName, NotificationDto notification)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
        }
    }
}
