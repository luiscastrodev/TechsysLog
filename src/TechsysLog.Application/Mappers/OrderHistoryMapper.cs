using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;
using TechsysLog.Domain.Entities;

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
                NewStatus: history.NewStatus,
                Reason: history.Reason,
                Notes: history.Notes,
                ChangedByUserId: history.ChangedByUserId,
                CreatedAt: history.CreatedAt
            );
        }
    }
}
