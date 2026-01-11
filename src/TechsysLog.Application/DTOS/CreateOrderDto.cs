using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Application.DTOS
{
    public record CreateOrderDto(
        [Required(ErrorMessage = "A descrição do pedido é obrigatória.")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "A descrição deve ter entre 5 e 250 caracteres.")]
        string Description,

        [Required(ErrorMessage = "O valor do pedido é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor do pedido deve ser maior que zero.")]
        decimal Amount,

        [Required(ErrorMessage = "Os dados de endereço são obrigatórios.")]
        AddressDto Address
      );
}
