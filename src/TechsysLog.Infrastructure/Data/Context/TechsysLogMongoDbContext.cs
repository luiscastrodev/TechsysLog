using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Infrastructure.Data.Context
{
    public class TechsysLogMongoDbContext : DbContext
    {
        public DbSet<User> Users { get; init; }
        public DbSet<RefreshToken> RefreshTokens { get; init; }
        public DbSet<Order> Orders { get; init; }
        public DbSet<OrderHistory> OrderHistories { get; init; }
        public DbSet<Notification> Notifications { get; init; }

        public TechsysLogMongoDbContext(DbContextOptions<TechsysLogMongoDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplica as configurações de mapeamento definidas neste projeto TechsysLogMongoDbContext
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TechsysLogMongoDbContext).Assembly);
        }
    }
}
