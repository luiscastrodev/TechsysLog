using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Domain.Entities.ENUMS
{
    /// <summary>
    /// Enum para representar o status da entrega.
    /// Controla o fluxo de tentativas e confirmação de entrega.
    /// </summary>
    public enum DeliveryStatus : byte
    {
        Pending = 0,       // Aguardando coleta
        InTransit = 1,     // Em trânsito
        OutForDelivery = 2, // Saiu para entrega
        Delivered = 3,     // Entregue com sucesso
        Failed = 4,        // Falha na entrega
        Returned = 5       // Devolvido ao remetente
    }
}
