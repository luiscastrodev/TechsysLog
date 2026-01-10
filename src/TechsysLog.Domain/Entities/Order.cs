using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.Common;
using TechsysLog.Domain.Entities.ENUMS;

namespace TechsysLog.Domain.Entities
{
    public class Order : Entity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public Address ShippingAddress { get; set; } = new();

        public Order() { }

        public Order(string orderNumber, Guid userId, string description,
            decimal amount, Address shippingAddress)
        {
            OrderNumber = orderNumber;
            UserId = userId;
            Description = description;
            Amount = amount;
            ShippingAddress = shippingAddress;
        }


        public void ChangeStatus(OrderStatus newStatus)
        {
            if ((byte)newStatus < (byte)Status && Status != OrderStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    $"Não é possível alterar o status de '{Status}' para '{newStatus}'");
            }
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Delivered || Status == OrderStatus.Returned)
            {
                throw new InvalidOperationException("Não é possível cancelar um pedido entregue");
            }
            Status = OrderStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
