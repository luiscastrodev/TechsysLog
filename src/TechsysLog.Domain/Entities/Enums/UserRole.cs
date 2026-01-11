using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Domain.Entities.Enums
{
    public enum UserRole : byte
    {
        [Description("Cliente")]
        User = 1,

        [Description("Operador")]
        Operator = 2,

        [Description("Administrador")]
        Admin = 3
    }
}
