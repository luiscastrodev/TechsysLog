using System;
using System.Collections.Generic;
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
        Pending = 0,      // Aguardando processamento
        Processing = 1,   // Em processamento
        Shipped = 2,      // Enviado
        Delivered = 3,    // Entregue
        Cancelled = 4,    // Cancelado
        Returned = 5      // Devolvido
    }
}
