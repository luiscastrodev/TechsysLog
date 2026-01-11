using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechsysLog.Application.Common;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.Api.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;
        public NotificationsController(INotificationService service) => _service = service;

        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            return Ok(await _service.GetUserNotificationsAsync(userId));
        }

        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            return Ok(await _service.GetUnreadCountAsync(userId));
        }

        [HttpPatch("{id}/read")]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _service.MarkAsReadAsync(id);
            if(result.IsSuccess == false)
                return BadRequest(result);  
            return NoContent();
        }

        [HttpPatch("read-all")]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _service.MarkAllAsReadAsync(userId);

            if(result.IsSuccess == false)
                return BadRequest(result);
            return NoContent();
        }
    }
}
