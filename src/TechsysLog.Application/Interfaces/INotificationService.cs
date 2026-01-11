using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Domain.Entities.ENUMS;

namespace TechsysLog.Application.Interfaces
{
    public interface INotificationService
    {
        Task<BusinessResult<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(Guid userId);
        Task<BusinessResult<int>> GetUnreadCountAsync(Guid userId);
        Task<BusinessResult<bool>> MarkAsReadAsync(Guid notificationId);
        Task<BusinessResult<bool>> MarkAllAsReadAsync(Guid userId);
    }
}
