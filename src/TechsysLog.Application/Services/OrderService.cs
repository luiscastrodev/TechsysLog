using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities.ENUMS;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICepService _cepService;
        private readonly INotificationRepository _notificationRepository;

        public OrderService(IOrderRepository orderRepository, ICepService cepService, INotificationRepository notificationRepository)
        {
            _orderRepository = orderRepository;
            _cepService = cepService;
            _notificationRepository = notificationRepository;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderDto dto)
        {
            var address = await _cepService.GetAddressByCepAsync(dto.Address.ZipCode);
            if (address == null) throw new Exception("CEP Inválido ou não encontrado.");

            address.Number = dto.Address.Number;

            var orderNumber = $"TECHSYS-{DateTime.UtcNow.Ticks.ToString().Substring(10)}";

            var order = new Order(orderNumber, userId, dto.Description, dto.Amount, address);

            await _orderRepository.AddAsync(order);

            var notification = new Notification(userId, "Novo Pedido", $"Seu pedido {orderNumber} foi criado com sucesso!", NotificationType.Success, order.Id, "Order");
            await _notificationRepository.AddAsync(notification);

            return new OrderResponseDto(order.Id, order.OrderNumber, order.Description, order.Amount, order.Status, null, order.CreatedAt);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return orders.Select(o => new OrderResponseDto(o.Id, o.OrderNumber, o.Description, o.Amount, o.Status, null, o.CreatedAt));
        }

        public async Task<OrderResponseDto?> GetByNumberAsync(string orderNumber)
        {
            var o = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            return o == null ? null : new OrderResponseDto(o.Id, o.OrderNumber, o.Description, o.Amount, o.Status, null, o.CreatedAt);
        }
    }
}
