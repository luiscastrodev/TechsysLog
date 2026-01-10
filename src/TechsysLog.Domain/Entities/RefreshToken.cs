using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.Common;

namespace TechsysLog.Domain.Entities
{
    public class RefreshToken : Entity
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }

        public bool IsActive => !IsRevoked && DateTime.UtcNow <= ExpiryDate;

        public RefreshToken() { }
        public RefreshToken(string token, Guid userId, DateTime expiryDate)
        {
            Token = token;
            UserId = userId;
            ExpiryDate = expiryDate;
        }

        public void Revoke()
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
        }

    }
}
