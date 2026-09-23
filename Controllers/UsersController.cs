
using System.Security.Claims;
using GridLink.DTOs;
using GridLink.Helpers;
using GridLink.Models;
using GridLink.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GridLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<List<UserResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? role, [FromQuery] string? status)
        {
            var result = await _userService.GetAllUsersAsync(role, status);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] UserRCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<UserResponseDTO>.FailureResult(
                    "Invalid payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _userService.CreateUserAsync(dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("pending-activations")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<List<UserResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingActivations()
        {
            var result = await _userService.GetPendingActivationsAsync();
            return Ok(result);
        }

        [HttpGet("nic/{nic}")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByNic(string nic)
        {
            var result = await _userService.GetUserByNicAsync(nic);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<UserResponseDTO>.FailureResult(
                    "Invalid payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _userService.UpdateUserAsync(id, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var result = await _userService.ActivateUserAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("me/profile")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized(ApiResponse<UserResponseDTO>.FailureResult("User not recognized from token."));
            }

            var result = await _userService.GetUserByIdAsync(currentUserId);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPut("me/profile")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<UserResponseDTO>.FailureResult(
                    "Invalid profile data.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized(ApiResponse<UserResponseDTO>.FailureResult("User not recognized from token."));
            }

            var result = await _userService.UpdateProfileAsync(currentUserId, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("me/change-password")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.FailureResult(
                    "Invalid password data.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized(ApiResponse<bool>.FailureResult("User not recognized from token."));
            }

            var result = await _userService.ChangePasswordAsync(currentUserId, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


    }
}
