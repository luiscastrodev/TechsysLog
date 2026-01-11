using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;

namespace TechsysLog.Application.Interfaces
{
    public interface IOrderService
    {
        Task<BusinessResult<OrderResponseDto>> CreateOrderAsync(Guid userId, CreateOrderDto dto);
        Task<BusinessResult<IEnumerable<OrderResponseDto>>> GetUserOrdersAsync(Guid userId);
            Task<BusinessResult<OrderResponseDto?>> GetByNumberAsync(string orderNumber);
        Task<BusinessResult<IEnumerable<OrderResponseDto>>> GetAllOrdersAsync();

    }
}
