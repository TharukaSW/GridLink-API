using GridLink.DTOs;
using GridLink.Helpers;
using GridLink.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GridLink.DTOs.AuthDTOs;

namespace GridLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register-prosumer")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterProsumer([FromBody] RegisterProsumerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<UserResponseDTO>.FailureResult(
                    "Invalid registration payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _authService.RegisterProsumerAsync(dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.FailureResult(
                    "Invalid login payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _authService.LoginAsync(dto);
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
    }
}
