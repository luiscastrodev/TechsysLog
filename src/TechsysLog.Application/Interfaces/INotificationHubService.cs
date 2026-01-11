using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;

namespace TechsysLog.Application.Interfaces
{
    public interface INotificationHubService
    {
        Task NotifyUserAsync(Guid userId, NotificationDto notification);
        Task NotifyOrderStatusChangeAsync(Guid userId, string orderNumber, string previousStatus, string newStatus);
        Task NotifyOrderDeliveryAsync(Guid userId, string orderNumber, string userReceived);
        Task NotifyNewOrderAsync(Guid userId, string orderNumber);
        Task NotifyAllAsync(NotificationDto notification);
        Task NotifyGroupAsync(string groupName, NotificationDto notification);
    }

}
