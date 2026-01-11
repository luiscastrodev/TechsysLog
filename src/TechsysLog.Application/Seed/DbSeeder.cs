using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Application.Seed
{
    using TechsysLog.Application.DTOS;
    using TechsysLog.Application.Interfaces;
    using TechsysLog.Domain.Interfaces;

    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IUserService userService, IUserRepository userRepository)
        {
            const string adminEmail = "operador@techsyslog.com";
            const string adminPassword = "Operador@123";
            const string adminName = "Operador Logistico";

            if (!await userRepository.EmailExistsAsync(adminEmail))
            {
                var createDto = new CreateUserDto
                {
                    Name = adminName,
                    Email = adminEmail,
                    Password = adminPassword,
                    Role = Domain.Entities.Enums.UserRole.Operator
                };

                await userService.RegisterAsync(createDto);
            }
        }
    }

}
