using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Application.Interfaces
{
    public interface IPasswordHasherService
    {
        bool VerifyPassword(string hashedPassword, string providedPassword);
        string HashPassword(string password);
    }
}
