using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.ENUMS;

namespace TechsysLog.Application.DTOS
{
    public record NotificationDto(
         Guid Id,
         string Title,
         string Message,
         NotificationType Type,
         bool IsRead,
         DateTime CreatedAt,
         Guid? RelatedEntityId = null,
         string? RelatedEntityType = null
     );
}
