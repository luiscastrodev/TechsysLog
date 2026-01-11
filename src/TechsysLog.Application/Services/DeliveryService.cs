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

namespace TechsysLog.Application.Services
{
    public class DeliveryService : BaseService, IDeliveryService
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationHubService _notificationHubService;

        public DeliveryService(IDeliveryRepository deliveryRepository, IOrderRepository orderRepository, INotificationRepository notificationRepository, INotificationHubService notificationHubService)
        {
            _deliveryRepository = deliveryRepository;
            _orderRepository = orderRepository;
            _notificationRepository = notificationRepository;
            _notificationHubService = notificationHubService;
        }

        public async Task<BusinessResult<bool>> RegisterDeliveryAsync(DeliveryDto dto)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(dto.OrderNumber);
            if (order == null) throw new Exception("Pedido não encontrado.");

            order.ChangeStatus(OrderStatus.Delivered);
            await _orderRepository.UpdateAsync(order);

            var delivery = new Delivery(order.OrderNumber, order.Id);
            delivery.MarkAsDelivered(dto.UserReceived);
            if (!string.IsNullOrEmpty(dto.Notes)) delivery.Notes = dto.Notes;

            await _deliveryRepository.AddAsync(delivery);

            var notification = new Notification(order.UserId, "Pedido Entregue", $"O pedido {order.OrderNumber} foi entregue para {dto.UserReceived}.", NotificationType.Info, order.Id, "Order");
            await _notificationRepository.AddAsync(notification);


            // Notificar via SignalR
            await _notificationHubService.NotifyOrderDeliveryAsync(
                order.UserId,
                order.OrderNumber,
                dto.UserReceived
            );


            return Success(true, "Entrega registrada com sucesso");
        }
    }
}
