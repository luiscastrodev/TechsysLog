using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Application.DTOS
{
    public record AddressDto(
        [Required(ErrorMessage = "O CEP é obrigatório.")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "O CEP informado é inválido (use 00000-000 ou 00000000).")]
        string ZipCode,

        [Required(ErrorMessage = "O número do endereço é obrigatório.")]
        [StringLength(20, ErrorMessage = "O número deve ter no máximo 20 caracteres.")]
        string Number,
        string? Neighborhood = null,
        string? Street = null,
        string? City = null,
        string? State = null
        );
}
