using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Domain.Entities.ENUMS;

namespace TechsysLog.Application.DTOS
{
    public record ChangeOrderStatusDto
    {
        [Required(ErrorMessage = "O novo status é obrigatório.")]
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "O status informado é inválido.")]
        public OrderStatus NewStatus { get; init; }

        [StringLength(500, ErrorMessage = "O motivo da alteração não pode exceder 500 caracteres.")]
        public string? Reason { get; init; }
    }
}
