using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using TechsysLog.Application.DTOS;

namespace TechsysLog.Application.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, string> UserConnections = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.ConnectionId;

            if (!UserConnections.ContainsKey(userId))
                UserConnections[userId] = Context.ConnectionId;

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.ConnectionId;

            if (UserConnections.ContainsKey(userId))
                UserConnections.Remove(userId);

            await base.OnDisconnectedAsync(exception);
        }

        // Notificar usuário específico
        public async Task NotifyUserAsync(string userId, NotificationDto notification)
        {
            if (UserConnections.TryGetValue(userId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
            }
        }

        // Notificar todos os usuários
        public async Task NotifyAllAsync(NotificationDto notification)
        {
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }


        // Notificar atualização de status do pedido
        public async Task NotifyOrderStatusChangeAsync(string userId, string orderNumber, string previousStatus, string newStatus)
        {
            var notification = new
            {
                OrderNumber = orderNumber,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                Timestamp = DateTime.UtcNow,
                Message = $"O pedido {orderNumber} mudou de {previousStatus} para {newStatus}."
            };

            if (UserConnections.TryGetValue(userId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("OrderStatusChanged", notification);
            }
        }

        // Notificar entrega de pedido
        public async Task NotifyOrderDeliveryAsync(string userId, string orderNumber, string userReceived)
        {
            var notification = new
            {
                OrderNumber = orderNumber,
                UserReceived = userReceived,
                Timestamp = DateTime.UtcNow,
                Message = $"O pedido {orderNumber} foi entregue para {userReceived}."
            };

            if (UserConnections.TryGetValue(userId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("OrderDelivered", notification);
            }
        }

        // Notificar novo pedido criado
        public async Task NotifyNewOrderAsync(string userId, string orderNumber)
        {
            var notification = new
            {
                OrderNumber = orderNumber,
                Timestamp = DateTime.UtcNow,
                Message = $"Seu pedido {orderNumber} foi criado com sucesso!"
            };

            if (UserConnections.TryGetValue(userId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("NewOrderCreated", notification);
            }
        }
    }
}
