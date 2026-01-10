using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;

namespace TechsysLog.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderDto dto);
        Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(Guid userId);
        Task<OrderResponseDto?> GetByNumberAsync(string orderNumber);
    }
}
