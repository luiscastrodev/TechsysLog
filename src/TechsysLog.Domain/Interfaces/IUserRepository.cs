using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);

        Task<bool> EmailExistsAsync(string email);

        Task<IEnumerable<User>> GetActiveUsersAsync();

        Task<int> GetActiveUsersCountAsync();
    }
}
