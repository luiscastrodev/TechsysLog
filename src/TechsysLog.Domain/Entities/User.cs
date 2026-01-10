using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.Common;

namespace TechsysLog.Domain.Entities
{
    public class User : Entity
    {

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Active { get; set; } = true;

        public User() { }
        public User(string name, string email, string password)
        {
            Name = name;
            Email = email;
            Password = password;
        }

        public bool CanLogin() => Active && !Deleted;

        public void Deactivate() => Active = false;
        public void Activate() => Active = true;
    }
}
