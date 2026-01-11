using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [ProducesResponseType(typeof(BusinessResult<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            if(dto.UserId.HasValue && dto.UserId.Value != Guid.Empty)
            {
                userId = dto.UserId.Value;
            }
            var order = await _orderService.CreateOrderAsync(userId, dto);

            return Ok(order);
        }

        [ProducesResponseType(typeof(BusinessResult<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            return Ok(await _orderService.GetUserOrdersAsync(userId));
        }

        [Authorize(Roles = "Operator,Admin")]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BusinessResult<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll() => Ok(await _orderService.GetAllOrdersAsync());


        [Authorize(Roles = "Operator,Admin")]
        [HttpPatch("{orderNumber}/status")]
        [ProducesResponseType(typeof(BusinessResult<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangeStatus(string orderNumber, [FromBody] ChangeOrderStatusDto dto)
        {
            var changedByUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var result = await _orderService.ChangeOrderStatusAsync(
                orderNumber,
                dto.NewStatus,
                changedByUserId,
                dto.Reason
            );

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}
