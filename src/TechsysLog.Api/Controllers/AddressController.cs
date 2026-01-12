using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly ICepService _cepService;

        public AddressController(ICepService cepService)
        {
            _cepService = cepService;
        }
        [ProducesResponseType(typeof(BusinessResult<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [HttpGet("search-zipcode")]
        public async Task<IActionResult> GetAddressByCepAsync(string zipcode)
        {
            return Ok(await _cepService.GetAddressByCepAsync(zipcode));
        }
    }
}
