using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces
{

    public interface IOrderHistoryRepository : IRepository<OrderHistory>
    {
        Task<IEnumerable<OrderHistory>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<OrderHistory>> GetChangesByUserAsync(Guid userId);

    }
}
