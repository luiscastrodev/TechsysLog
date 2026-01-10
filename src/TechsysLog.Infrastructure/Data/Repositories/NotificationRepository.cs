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
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(
            TechsysLogMongoDbContext context,
            ILogger<NotificationRepository> logger)
            : base(context, logger) { }

        /// <summary>
        /// Busca todas as notificações de um usuário.
        /// </summary>
        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Buscando notificacoes do usuario: {UserId}", userId);

            return await _dbSet
                .Where(n => n.UserId == userId && !n.Deleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Busca apenas as notificações não lidas de um usuário.
        /// </summary>
        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Busca apenas as notificações não lidas de um usuário: {UserId}", userId);

            return await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead && !n.Deleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Conta quantas notificações não lidas um usuário tem.
        /// </summary>
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            _logger.LogInformation("Conta quantas notificações não lidas um usuário tem: {UserId}", userId);

            return await _dbSet
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.Deleted);
        }

        /// <summary>
        /// Marca uma notificação específica como lida.
        /// </summary>
        public async Task MarkAsReadAsync(Guid notificationId)
        {
            _logger.LogInformation("Marca uma notificação específica como lida: {NotificationId}", notificationId);

            var notification = await GetByIdAsync(notificationId);
            if (notification != null)
            {
                notification.MarkAsRead();
                await UpdateAsync(notification);
            }
        }

        /// <summary>
        /// Marca todas as notificações de um usuário como lidas.
        /// </summary>
        public async Task MarkAllAsReadAsync(Guid userId)
        {
            _logger.LogInformation("Marca todas as notificações de um usuário como lidas.: {UserId}", userId);

            var unreadNotifications = await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead && !n.Deleted)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.MarkAsRead();
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Busca notificações de um tipo específico para um usuário.
        /// </summary>
        public async Task<IEnumerable<Notification>> GetByTypeAsync(Guid userId, NotificationType type)
        {
            _logger.LogInformation("  Busca notificações de um {Type} para um usuário.: {UserId}", type, userId);

            return await _dbSet
                .Where(n => n.UserId == userId && n.Type == type && !n.Deleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
