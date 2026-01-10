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

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(
            TechsysLogMongoDbContext context,
            ILogger<UserRepository> logger)
            : base(context, logger) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Searching user by email: {Email}", email);

            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email && !u.Deleted);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            _logger.LogInformation("Checking if email exists: {Email}", email);

            return await _dbSet
                .AnyAsync(u => u.Email == email && !u.Deleted);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            _logger.LogInformation("Getting all active users");

            return await _dbSet
                .Where(u => u.Active && !u.Deleted)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            _logger.LogInformation("Counting active users");

            return await _dbSet
                .CountAsync(u => u.Active && !u.Deleted);
        }
    }
}
