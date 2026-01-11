using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;

namespace TechsysLog.Application.Interfaces
{
    public interface IUserService
    {
        Task<BusinessResult<UserResponseDto>> RegisterAsync(CreateUserDto dto);
    }
}
