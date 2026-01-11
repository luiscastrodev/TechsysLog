using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;

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
    }
}
