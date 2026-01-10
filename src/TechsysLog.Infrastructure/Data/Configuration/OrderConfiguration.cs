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
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToCollection("orders");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber).IsRequired();
            builder.Property(o => o.Description).IsRequired();

            builder.OwnsOne(o => o.ShippingAddress, nav =>
            {
                nav.Property(a => a.ZipCode).IsRequired();
                nav.Property(a => a.Street).IsRequired();
                nav.Property(a => a.Number).IsRequired();
                nav.Property(a => a.Neighborhood).IsRequired();
                nav.Property(a => a.City).IsRequired();
                nav.Property(a => a.State).IsRequired();
            });

            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.HasIndex(o => o.UserId);
            builder.HasIndex(o => o.Status);
            builder.HasIndex(o => o.Deleted);
            builder.HasIndex(o => new { o.UserId, o.Deleted });
        }
    }
}
