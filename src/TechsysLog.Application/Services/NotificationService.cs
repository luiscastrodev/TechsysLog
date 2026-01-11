using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities.ENUMS;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Common;

namespace TechsysLog.Application.Services
{
    public class NotificationService : BaseService, INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<BusinessResult<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(Guid userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);

            return Success(notifications.Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt,
                n.RelatedEntityId,
                n.RelatedEntityType
            )));
        }

        public async Task<BusinessResult<int>> GetUnreadCountAsync(Guid userId)
        {
            var result = await _notificationRepository.GetUnreadCountAsync(userId);
            return Success(result);
        }

        public async Task<BusinessResult<bool>> MarkAsReadAsync(Guid notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
            return Success(true);
        }

        public async Task<BusinessResult<bool>> MarkAllAsReadAsync(Guid userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);

            return Success(true);
        }
    }
}
