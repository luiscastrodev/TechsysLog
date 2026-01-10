using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.Common;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.Data.Context;

namespace TechsysLog.Infrastructure.Data.Common
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        protected readonly TechsysLogMongoDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger _logger;

        protected Repository(TechsysLogMongoDbContext context, ILogger logger)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

        public async Task AddAsync(T entity)
        {
            _logger.LogInformation("Adding entity of type {Type} with Id {Id}", typeof(T).Name, entity.Id);
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _logger.LogInformation("Updating entity of type {Type} with Id {Id}", typeof(T).Name, entity.Id);
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _logger.LogWarning("Performing Soft Delete on entity of type {Type} with Id {Id}", typeof(T).Name, id);
                entity.Deleted = true;
                await UpdateAsync(entity);
            }
        }
    }
}
