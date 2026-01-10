using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.Common;
using TechsysLog.Domain.Entities.ENUMS;

namespace TechsysLog.Domain.Entities
{
    public class Notification : Entity
    {
        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; } = NotificationType.Info;

        public Guid? RelatedEntityId { get; set; }

        public string? RelatedEntityType { get; set; }

        public DateTime? ReadAt { get; set; }
        public bool IsRead => ReadAt.HasValue;

        public Notification() { }

        public Notification(Guid userId, string title, string message,
            NotificationType type = NotificationType.Info,
            Guid? relatedEntityId = null, string? relatedEntityType = null)
        {
            UserId = userId;
            Title = title;
            Message = message;
            Type = type;
            RelatedEntityId = relatedEntityId;
            RelatedEntityType = relatedEntityType;
        }

        public void MarkAsRead()
        {
            ReadAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        public void MarkAsUnread()
        {
            ReadAt = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
