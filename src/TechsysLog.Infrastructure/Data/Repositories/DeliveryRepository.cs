using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.ENUMS;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.Data.Common;
using TechsysLog.Infrastructure.Data.Context;

namespace TechsysLog.Infrastructure.Data.Repositories
{
    public class DeliveryRepository : Repository<Delivery>, IDeliveryRepository
    {
        public DeliveryRepository(
            TechsysLogMongoDbContext context,
            ILogger<DeliveryRepository> logger)
            : base(context, logger) { }

        /// <summary>
        /// Busca a entrega de um pedido.
        /// </summary>
        public async Task<Delivery?> GetByOrderIdAsync(Guid orderId)
        {
            _logger.LogInformation("Busca a entrega de um pedido.: {OrderId}", orderId);

            return await _dbSet
                .FirstOrDefaultAsync(d => d.OrderId == orderId && !d.Deleted);
        }

        /// <summary>
        /// Busca entregas por status.
        /// </summary>
        public async Task<IEnumerable<Delivery>> GetByStatusAsync(DeliveryStatus status)
        {
            _logger.LogInformation("Busca entregas por status.: {Status}", status);

            return await _dbSet
                .Where(d => d.Status == status && !d.Deleted)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Busca entregas que falharam.
        /// </summary>
        public async Task<IEnumerable<Delivery>> GetFailedDeliveriesAsync()
        {
            _logger.LogInformation("Busca entregas que falharam.");

            return await _dbSet
                .Where(d => d.Status == DeliveryStatus.Failed && !d.Deleted)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Busca entregas recentes (últimos N dias).
        /// </summary>
        public async Task<IEnumerable<Delivery>> GetRecentDeliveriesAsync(int days = 7)
        {
            _logger.LogInformation("Busca entregas recentes {Days} days", days);

            var startDate = DateTime.UtcNow.AddDays(-days);

            return await _dbSet
                .Where(d => d.CreatedAt >= startDate && !d.Deleted)
                .OrderByDescending(d => d.DeliveredAt ?? d.CreatedAt)
                .ToListAsync();
        }
    }
}
