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

    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(TechsysLogMongoDbContext context, ILogger<RefreshTokenRepository> logger) : base(context, logger) { }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task RevokeTokensByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Revoking all tokens for User {UserId}", userId);
            var tokens = await _dbSet.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            await _context.SaveChangesAsync();
        }
    }
}
