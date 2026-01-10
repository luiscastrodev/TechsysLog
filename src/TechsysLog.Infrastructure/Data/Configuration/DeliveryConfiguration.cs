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
    public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
    {
        public void Configure(EntityTypeBuilder<Delivery> builder)
        {
            builder.ToCollection("registered_deliveries");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.OrderNumber).IsRequired();
            builder.Property(d => d.OrderId).IsRequired();

            builder.HasIndex(d => d.OrderId).IsUnique();
            builder.HasIndex(d => d.Status);
            builder.HasIndex(d => d.Deleted);
            builder.HasIndex(d => d.CreatedAt);
        }
    }
}
