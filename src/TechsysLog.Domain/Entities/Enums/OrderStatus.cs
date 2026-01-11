using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Domain.Entities.ENUMS
{
    /// <summary>
    /// Enum para representar o status do pedido.
    /// Facilita validações, evita valores inválidos e permite buscar por status.
    /// </summary>
    public enum OrderStatus : byte
    {
        [Description("Aguardando processamento")]Pending = 0,
        [Description("Em processamento")]Processing = 1,
        [Description("Enviado")] Shipped = 2, 
        [Description("Entregue")]Delivered = 3,  
        [Description("Cancelado")] Cancelled = 4, 
        [Description("Devolvido")] Returned = 5  
    }
}
