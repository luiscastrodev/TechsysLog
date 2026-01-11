using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.Api.Controllers
{
    [Authorize(Roles = "Operator,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;
        public DeliveryController(IDeliveryService deliveryService) => _deliveryService = deliveryService;

        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [HttpPost("register")]
        public async Task<IActionResult> Register(DeliveryDto dto)
        {
           var result = await _deliveryService.RegisterDeliveryAsync(dto);
            if (!result.IsSuccess) BadRequest(result);
            return Ok(result);
        }
    }
}
