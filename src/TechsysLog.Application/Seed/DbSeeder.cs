using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.Entities.Enums;

namespace TechsysLog.Application.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            IUserService userService,
            IUserRepository userRepository,
            IOrderService orderService)
        {
            await SeedAdminAsync(userService, userRepository);

            var users = await SeedFakeUsersAsync(userService, userRepository);

            await SeedFakeOrdersAsync(orderService, users);

        }

        private static async Task SeedAdminAsync(IUserService userService, IUserRepository userRepository)
        {
            const string adminEmail = "operador@techsyslog.com";
            if (!await userRepository.EmailExistsAsync(adminEmail))
            {
                await userService.RegisterAsync(new CreateUserDto
                {
                    Name = "Operador Logistico",
                    Email = adminEmail,
                    Password = "Operador@123",
                    Role = UserRole.Operator
                });
            }
        }

        private static async Task<List<Guid>> SeedFakeUsersAsync(IUserService userService, IUserRepository userRepository)
        {
            var userIds = new List<Guid>();
            var fakeUsers = new List<(string Name, string Email)>
            {
                ("João Silva", "joao.silva@email.com"),
                ("Maria Oliveira", "maria.o@email.com"),
                ("Carlos Souza", "carlos.souza@email.com"),
                ("Ana Costa", "ana.costa@email.com")
            };

            foreach (var fake in fakeUsers)
            {
                var existingUser = await userRepository.GetByEmailAsync(fake.Email);
                if (existingUser == null)
                {
                    var result = await userService.RegisterAsync(new CreateUserDto
                    {
                        Name = fake.Name,
                        Email = fake.Email,
                        Password = "User@123",
                        Role = UserRole.User
                    });
                    userIds.Add(result.Data.Id);
                }
                else
                {
                    userIds.Add(existingUser.Id);
                }
            }
            return userIds;
        }

        private static async Task SeedFakeOrdersAsync(IOrderService orderService, List<Guid> userIds)
        {
            var existingOrders = await orderService.GetAllOrdersAsync();
            if (existingOrders.Data.Any())
            {
                return;
            }

            var random = new Random();
            var descriptions = new[] { "Smartphone Samsung S23", "Notebook Dell Inspiron", "Cadeira Gamer", "Monitor LG 29'", "Teclado Mecânico RGB" };

            // Endereços Reais
            var addresses = new[]
            {
                new AddressDto("01001-000", "100", "Sé", "Praça da Sé", "São Paulo", "SP"),
                new AddressDto("20040-002", "500", "Centro", "Avenida Rio Branco", "Rio de Janeiro", "RJ"),
                new AddressDto("30140-071", "12", "Boa Viagem", "Rua Sergipe", "Belo Horizonte", "MG")
            };

            foreach (var userId in userIds)
            {
                // Criar 2 pedidos para cada usuário
                for (int i = 0; i < 2; i++)
                {
                    var addr = addresses[random.Next(addresses.Length)];
                    var createOrderDto = new CreateOrderDto(
                        Description: descriptions[random.Next(descriptions.Length)],
                        Amount: (decimal)(random.NextDouble() * 1500 + 50),
                        Address: addr,
                        UserId: userId
                    );

                    await orderService.CreateOrderAsync(userId, createOrderDto);
                }
            }
        }
    }
}