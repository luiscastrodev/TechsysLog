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
using TechsysLog.Application.Common;
using TechsysLog.Application.Mappers;

namespace TechsysLog.Application.Services
{
    public class OrderService : BaseService, IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICepService _cepService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IOrderHistoryRepository _orderHistoryRepository;
        private readonly INotificationHubService _notificationHubService;

        public OrderService(IOrderRepository orderRepository, ICepService cepService, INotificationRepository notificationRepository, IOrderHistoryRepository orderHistoryRepository, INotificationHubService notificationHubService)
        {
            _orderRepository = orderRepository;
            _cepService = cepService;
            _notificationRepository = notificationRepository;
            _orderHistoryRepository = orderHistoryRepository;
            _notificationHubService = notificationHubService;
        }

        public async Task<BusinessResult<OrderResponseDto>> CreateOrderAsync(Guid userId, CreateOrderDto dto)
        {
            var address = await _cepService.GetAddressByCepAsync(dto.Address.ZipCode);
            if (address == null) throw new Exception("CEP Inválido ou não encontrado.");

            address.Number = dto.Address.Number;

            var orderNumber = $"TECHSYS-{DateTime.UtcNow.Ticks.ToString().Substring(10)}";

            var order = new Order(orderNumber, userId, dto.Description, dto.Amount, address);

            await _orderRepository.AddAsync(order);

            var notification = new Notification(userId, "Novo Pedido", $"Seu pedido {orderNumber} foi criado com sucesso!", NotificationType.Success, order.Id, "Order");
            await _notificationRepository.AddAsync(notification);

            // Notificar via SignalR 
            await _notificationHubService.NotifyNewOrderAsync(userId, orderNumber);

            return Success(order.ToDto(), "Pedido Criado com sucesso.");
        }
        public async Task<BusinessResult<IEnumerable<OrderResponseDto>>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);

            var ordersWithHistory = await Task.WhenAll(
                orders.Select(async order =>
                {
                    var history = await _orderHistoryRepository.GetByOrderIdAsync(order.Id);
                    return order.ToDto(history);
                })
            );

            return Success(ordersWithHistory.AsEnumerable());
        }


        public async Task<BusinessResult<OrderResponseDto?>> GetByNumberAsync(string orderNumber)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            if (order == null)
                return Success<OrderResponseDto?>(null);

            var history = await _orderHistoryRepository.GetByOrderIdAsync(order.Id);
            return Success(order?.ToDto(history));
        }

        public async Task<BusinessResult<IEnumerable<OrderResponseDto>>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();

            var ordersWithHistory = new List<OrderResponseDto>();
            foreach (var order in orders)
            {
                var history = await _orderHistoryRepository.GetByOrderIdAsync(order.Id);
                ordersWithHistory.Add(order.ToDto(history));
            }

            return Success(ordersWithHistory.AsEnumerable());
        }


        public async Task<BusinessResult<OrderResponseDto>> ChangeOrderStatusAsync(string orderNumber, OrderStatus newStatus, Guid changedByUserId, string? reason = null)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            if (order == null)
                throw new BusinessException("Pedido não encontrado.");

            var previousStatus = order.Status;
            order.ChangeStatus(newStatus);

            await _orderRepository.UpdateAsync(order);

            var history = new OrderHistory(
                orderId: order.Id,
                previousStatus: previousStatus,
                newStatus: newStatus,
                changedByUserId: changedByUserId,
                reason: reason
            );

            await _orderHistoryRepository.AddAsync(history);

            var notification = new Notification(
                order.UserId,
                "Status do pedido alterado",
                $"O pedido {order.OrderNumber} mudou de {previousStatus} para {newStatus}.",
                NotificationType.Info,
                order.Id,
                "Order"
            );
            await _notificationRepository.AddAsync(notification);

            // Notificar via SignalR 
            await _notificationHubService.NotifyOrderStatusChangeAsync(
                order.UserId,
                order.OrderNumber,
                previousStatus.ToString(),
                newStatus.ToString()
            );
            return Success(order.ToDto(), "Status alterado com sucesso.");
        }


    }
}
