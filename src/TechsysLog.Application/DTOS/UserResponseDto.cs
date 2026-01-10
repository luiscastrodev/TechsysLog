using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Application.DTOS
{
    public record UserResponseDto(Guid Id, string Name, string Email, bool Active);
}
