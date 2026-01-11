using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Entities.Enums;

namespace TechsysLog.Application.Mappers
{
    public static class OrderHistoryMapper
    {
        public static OrderHistoryDto ToDto(this OrderHistory history)
        {
            if (history == null) return null!;

            return new OrderHistoryDto(
                Id: history.Id,
                PreviousStatus: history.PreviousStatus,
                PreviousStatusDescription: history.PreviousStatus.GetDescription(),
                NewStatus: history.NewStatus,
                NewsStatusDescription: history.NewStatus.GetDescription(),
                Reason: history.Reason,
                Notes: history.Notes,
                ChangedByUserId: history.ChangedByUserId,
                CreatedAt: history.CreatedAt
            );
        }
    }
}
