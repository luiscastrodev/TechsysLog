using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;

namespace TechsysLog.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService) { _userService = userService; }

        [ProducesResponseType(typeof(BusinessResult<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(CreateUserDto user) => Ok(await _userService.RegisterAsync(user));


        [ProducesResponseType(typeof(BusinessResult<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userService.GetAllAsync());
        }

        [ProducesResponseType(typeof(BusinessResult<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BusinessResult<string>), StatusCodes.Status404NotFound)]

        [HttpGet]
        public async Task<IActionResult> Get(Guid  userId)
        {
            var result = await _userService.GetById(userId);

            if (!result.IsSuccess) return NotFound();

            return Ok(result);
        }
    }
}
