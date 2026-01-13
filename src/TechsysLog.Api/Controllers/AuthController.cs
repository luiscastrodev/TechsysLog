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
    public class AuthController : ControllerBase
    {
        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }
        private readonly IAuthenticationService _authService;

        [ProducesResponseType(typeof(BusinessResult<AuthenticationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<AuthenticationResponseDTO>), StatusCodes.Status400BadRequest)]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] AuthenticationRequestDTo request)
        {
            var user = await _authService.LoginAsync(request.Login, request.Password);
            return Ok(user);
        }


        [ProducesResponseType(typeof(BusinessResult<AuthenticationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<AuthenticationResponseDTO>), StatusCodes.Status400BadRequest)]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            var result = await _authService.RefreshToken(request.Token);

            if (!result.IsSuccess)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [ProducesResponseType(typeof(BusinessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BusinessResult<bool>), StatusCodes.Status400BadRequest)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDTO request)
        {
            var result = await _authService.Logout(request.Token);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
