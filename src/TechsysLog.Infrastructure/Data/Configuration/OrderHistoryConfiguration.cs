using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Infrastructure.Data.Configuration
{
    public class OrderHistoryConfiguration : IEntityTypeConfiguration<OrderHistory>
    {
        public void Configure(EntityTypeBuilder<OrderHistory> builder)
        {
            builder.ToCollection("order_histories");
            builder.HasKey(h => h.Id);

            
            builder.Property(h => h.OrderId).IsRequired();
            builder.Property(h => h.ChangedByUserId).IsRequired();

            builder.HasIndex(h => h.OrderId);
            builder.HasIndex(h => h.ChangedByUserId);
            builder.HasIndex(h => h.Deleted);
            builder.HasIndex(h => new { h.OrderId, h.Deleted });
        }
    }
}
