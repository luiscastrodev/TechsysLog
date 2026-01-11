using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Application.DTOS
{
    public class RefreshTokenRequestDTO
    {
        [Required(ErrorMessage = "O campo Token é obrigatório.")]
        public string Token { get; set; } = string.Empty;
    }
}
