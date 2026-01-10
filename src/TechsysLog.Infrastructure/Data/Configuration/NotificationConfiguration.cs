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
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToCollection("notifications");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.UserId).IsRequired();
            builder.Property(n => n.Title).IsRequired();
            builder.Property(n => n.Message).IsRequired();

            builder.HasIndex(n => n.UserId);
            builder.HasIndex(n => n.Type);
            builder.HasIndex(n => n.Deleted);
            builder.HasIndex(n => new { n.UserId, n.Deleted });
            builder.HasIndex(n => new { n.UserId, n.ReadAt });
            builder.HasIndex(n => n.CreatedAt).IsDescending();
        }
    }
}
