using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.ENUMS;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces
{
    public interface IDeliveryRepository : IRepository<Delivery>
    {
        Task<Delivery?> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Delivery>> GetByStatusAsync(DeliveryStatus status);
        Task<IEnumerable<Delivery>> GetFailedDeliveriesAsync();
        Task<IEnumerable<Delivery>> GetRecentDeliveriesAsync(int days = 7);
    }
}
