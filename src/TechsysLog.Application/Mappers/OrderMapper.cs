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
    public static class OrderMapper
    {
        public static OrderResponseDto ToDto(this Order order, IEnumerable<OrderHistory>? history = null)
        {
            return new OrderResponseDto(
                Id: order.Id,
                OrderNumber: order.OrderNumber,
                Description: order.Description,
                Amount: order.Amount,
                Status: order.Status,
                StatusDescription: order.Status.GetDescription(),
                ShippingAddress: order.ShippingAddress.ToDto(),
                CreatedAt: order.CreatedAt,
                History: history?.Select(h => h.ToDto()) ?? Enumerable.Empty<OrderHistoryDto>()
            );
        }

        public static Order ToEntity(this OrderResponseDto dto, Guid userId)
        {
            if (dto == null) return null!;

            return new Order(
                orderNumber: dto.OrderNumber,
                userId: userId,
                description: dto.Description,
                amount: dto.Amount,
                shippingAddress: dto.ShippingAddress.ToEntity()
            )
            {
                Id = dto.Id
            };
        }
    }

}
