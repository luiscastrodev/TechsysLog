using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.Common;
using TechsysLog.Domain.Entities.ENUMS;

namespace TechsysLog.Domain.Entities
{
    public class Delivery : Entity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public Guid OrderId { get; set; }

        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

        public DateTime? DeliveredAt { get; set; }

        public string Notes { get; set; } = string.Empty;
        public byte Attempts { get; set; }
        public byte MaxAttempts { get; set; } = 3;

        public string? UserReceived { get; set; }
        public string? Signature { get; set; } //Futuro Pode ser uma assinatura salvando a url ou a imagem em base64

        public Delivery() { }

        public Delivery(string orderNumber, Guid orderId)
        {
            OrderNumber = orderNumber;
            OrderId = orderId;
        }

        public void RegisterAttempt(string notes)
        {
            Attempts++;
            Notes = notes;
            if (Attempts >= MaxAttempts)
                Status = DeliveryStatus.Failed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsDelivered(string userReceived, string? signature = null)
        {
            Status = DeliveryStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
            UserReceived = userReceived;
            Signature = signature;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool CanRetry() => Status != DeliveryStatus.Delivered && Attempts < MaxAttempts;
    }
}