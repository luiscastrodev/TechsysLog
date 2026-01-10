using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.Data.Common;
using TechsysLog.Infrastructure.Data.Context;

namespace TechsysLog.Infrastructure.Data.Repositories
{
    
    public class OrderHistoryRepository : Repository<OrderHistory>, IOrderHistoryRepository
    {
        public OrderHistoryRepository(
            TechsysLogMongoDbContext context,
            ILogger<OrderHistoryRepository> logger)
            : base(context, logger) { }

        /// <summary>
        /// Busca todo o histórico de um pedido.
        /// </summary>
        public async Task<IEnumerable<OrderHistory>> GetByOrderIdAsync(Guid orderId)
        {
            _logger.LogInformation("Getting history for order: {OrderId}", orderId);

            return await _dbSet
                .Where(oh => oh.OrderId == orderId && !oh.Deleted)
                .OrderBy(oh => oh.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Busca todas as alterações feitas por um usuário.
        /// </summary>
        public async Task<IEnumerable<OrderHistory>> GetChangesByUserAsync(Guid userId)
        {
            _logger.LogInformation("Getting changes made by user: {UserId}", userId);

            return await _dbSet
                .Where(oh => oh.ChangedByUserId == userId && !oh.Deleted)
                .OrderByDescending(oh => oh.CreatedAt)
                .ToListAsync();
        }
    }
}
