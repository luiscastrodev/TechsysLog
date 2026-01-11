using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Domain.Entities.Enums
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());

            if (field == null) return value.ToString();

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute != null ? attribute.Description : value.ToString();
        }
    }
}
