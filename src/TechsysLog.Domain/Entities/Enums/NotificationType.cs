using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Domain.Entities.ENUMS
{
    /// <summary>
    /// Enum para tipo de notificação.
    /// Permite categorizar e filtrar notificações por tipo.
    /// </summary>
    public enum NotificationType : byte
    {
        Info = 0,           // Informação geral
        Warning = 1,        // Aviso
        Error = 2,          // Erro
        Success = 3,        // Sucesso
        OrderUpdate = 4,    // Atualização de pedido
        DeliveryUpdate = 5  // Atualização de entrega
    }
}
