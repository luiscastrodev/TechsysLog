using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Entities.ENUMS;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.Data.Common;
using TechsysLog.Infrastructure.Data.Context;

namespace TechsysLog.Infrastructure.Data.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(
            TechsysLogMongoDbContext context,
            ILogger<OrderRepository> logger)
            : base(context, logger) { }

        /// <summary>
        /// Busca um pedido por número.
        /// </summary>
        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            _logger.LogInformation("Buscando pedido por id: {OrderNumber}", orderNumber);

            return await _dbSet
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && !o.Deleted);
        }

        /// <summary>
        /// Busca todos os pedidos de um usuário.
        /// </summary>
        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Buscando pedidos por usuario: {UserId}", userId);

            return await _dbSet
                .Where(o => o.UserId == userId && !o.Deleted)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Busca pedidos por status.
        /// </summary>
        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            _logger.LogInformation("Buscando orders por status: {Status}", status);

            return await _dbSet
                .Where(o => o.Status == status && !o.Deleted)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Conta quantos pedidos um usuário tem.
        /// </summary>
        public async Task<int> GetOrderCountByUserAsync(Guid userId)
        {
            _logger.LogInformation("Contando quantidade de pedidos por usuario: {UserId}", userId);

            return await _dbSet
                .CountAsync(o => o.UserId == userId && !o.Deleted);
        }
    }
}
