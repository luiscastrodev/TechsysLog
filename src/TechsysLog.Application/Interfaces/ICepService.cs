using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Application.Interfaces
{
    public interface ICepService
    {
        Task<Address?> GetAddressByCepAsync(string cep);
    }
}
