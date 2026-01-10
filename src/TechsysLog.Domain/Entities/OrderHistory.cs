using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.Common;
using TechsysLog.Domain.Entities.ENUMS;

namespace TechsysLog.Domain.Entities
{
  
    public class OrderHistory : Entity
    {
        public Guid OrderId { get; set; }

        public OrderStatus PreviousStatus { get; set; }

        public OrderStatus NewStatus { get; set; }

        public string? Reason { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Guid ChangedByUserId { get; set; }

        public OrderHistory() { }

        public OrderHistory(Guid orderId, OrderStatus previousStatus,
            OrderStatus newStatus, Guid changedByUserId, string? reason = null, string notes = "")
        {
            OrderId = orderId;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            ChangedByUserId = changedByUserId;
            Reason = reason;
            Notes = notes;
        }
    }
}
