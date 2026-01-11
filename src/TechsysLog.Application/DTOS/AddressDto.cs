using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Application.DTOS
{
    public record AddressDto(string ZipCode, string Number, string? Neighborhood = null,string? Street = null, string? City = null, string? State = null);

}
