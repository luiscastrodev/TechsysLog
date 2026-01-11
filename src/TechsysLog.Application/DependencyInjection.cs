using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ICepService, ViaCepService>();
            services.AddScoped<IDeliveryService, DeliveryService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationHubService, NotificationHubService>();

            return services;
        }
    }
}
