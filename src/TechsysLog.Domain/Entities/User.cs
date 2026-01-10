using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.Common;
using TechsysLog.Domain.Entities.Enums;

namespace TechsysLog.Domain.Entities
{
    public class User : Entity
    {

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime? LockoutEnd { get; set; } = null;
        public int FailedLoginAttempts { get; set; } = 0;

        public User() { }
        public User(string name, string email, string password, UserRole userRole = UserRole.User, DateTime? lockoutEnd = null , int failedLoginAttempts = 0)
        {
            Name = name;
            Email = email;
            PasswordHash = password;
            Role = userRole;
            LockoutEnd = LockoutEnd;
            FailedLoginAttempts = failedLoginAttempts;
        }

        public bool CanLogin() => Active && !Deleted;

        public void Deactivate() => Active = false;
        public void Activate() => Active = true;
    }
}
