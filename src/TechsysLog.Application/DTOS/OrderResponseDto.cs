using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.ENUMS;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Application.DTOS
{
    public record OrderResponseDto(
    Guid Id,
    string OrderNumber,
    string Description,
    decimal Amount,
    OrderStatus Status,
    string StatusDescription,
    AddressDto ShippingAddress,
    DateTime CreatedAt,
    IEnumerable<OrderHistoryDto> History);


}
