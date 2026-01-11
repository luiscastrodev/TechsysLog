using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Application.DTOS
{
    public record DeliveryDto(string OrderNumber, DateTime DeliveredAt, string UserReceived, string? Notes);

}
