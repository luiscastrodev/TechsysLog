using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.ENUMS;

namespace TechsysLog.Application.DTOS
{
    public record OrderHistoryDto(
    Guid Id,
    OrderStatus PreviousStatus,
    string PreviousStatusDescription,
    OrderStatus NewStatus,
    string NewsStatusDescription,
    string? Reason,
    string Notes,
    Guid ChangedByUserId,
    DateTime CreatedAt);

}
