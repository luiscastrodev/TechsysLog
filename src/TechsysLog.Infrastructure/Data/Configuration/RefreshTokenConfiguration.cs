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
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToCollection("refresh_tokens");
            builder.HasKey(t => t.Id);

            
            builder.Property(t => t.Token).IsRequired();

            builder.HasIndex(t => t.Token).IsUnique();
            builder.HasIndex(t => t.UserId);
            builder.HasIndex(t => t.ExpiryDate);
            builder.HasIndex(t => new { t.UserId, t.IsRevoked });
        }
    }
}
