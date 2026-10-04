using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GridLink.DTOs;
using GridLink.Models;
using GridLink.Services.Interfaces;
using GridLink.Helpers;

namespace GridLink.Controllers
{
    [Route("api/prosumers")]
    [ApiController]
    public class ProsumersController : ControllerBase
    {
        private readonly IProsumerService _prosumerService;

        public ProsumersController(IProsumerService prosumerService)
        {
            _prosumerService = prosumerService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] ProsumerRegisterRequestDTO request)
        {
            var (statusCode, message, data) = await _prosumerService.RegisterAsync(request);
            return MapResponse(statusCode, message, data);
        }

        [HttpGet]
        [Authorize(Roles = UserRole.BackOffice + "," + UserRole.GridOperator)]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var (statusCode, message, data) = await _prosumerService.GetAllAsync(status);
            return MapResponse(statusCode, message, data);
        }

        [HttpGet("{nic}")]
        [Authorize]
        public async Task<IActionResult> GetByNic(string nic)
        {
            var callerNic = User.FindFirst("nic")?.Value;
            var callerRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var (statusCode, message, data) = await _prosumerService.GetByNicAsync(nic, callerNic, callerRole);
            return MapResponse(statusCode, message, data);
        }

        [HttpPut("{nic}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(string nic, [FromBody] ProsumerUpdateRequestDTO request)
        {
            var callerNic = User.FindFirst("nic")?.Value;
            var callerRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var (statusCode, message, data) = await _prosumerService.UpdateProfileAsync(nic, request, callerNic, callerRole);
            return MapResponse(statusCode, message, data);
        }

        [HttpPatch("{nic}/request-deactivation")]
        [Authorize(Roles = UserRole.Prosumer)]
        public async Task<IActionResult> RequestDeactivation(string nic)
        {
            var callerNic = User.FindFirst("nic")?.Value;
            var callerRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var (statusCode, message) = await _prosumerService.RequestDeactivationAsync(nic, callerNic, callerRole);
            return MapResponse<object>(statusCode, message, null);
        }

        [HttpPatch("{nic}/deactivate")]
        [Authorize(Roles = UserRole.BackOffice + "," + UserRole.GridOperator)]
        public async Task<IActionResult> Deactivate(string nic)
        {
            var (statusCode, message) = await _prosumerService.DeactivateAsync(nic);
            return MapResponse<object>(statusCode, message, null);
        }

        [HttpPatch("{nic}/activate")]
        [Authorize(Roles = UserRole.BackOffice)]
        public async Task<IActionResult> Activate(string nic)
        {
            var (statusCode, message) = await _prosumerService.ActivateAsync(nic);
            return MapResponse<object>(statusCode, message, null);
        }

        private IActionResult MapResponse<T>(int statusCode, string message, T? data = default)
        {
            bool isSuccess = statusCode >= 200 && statusCode < 300;
            object response;

            if (isSuccess)
            {
                response = data != null 
                    ? ApiResponse<T>.SuccessResult(data, message) 
                    : ApiResponse.SuccessResult(message);
            }
            else
            {
                response = ApiResponse<T>.FailureResult(message);
            }

            return statusCode switch
            {
                200 => Ok(response),
                201 => Created(string.Empty, response),
                400 => BadRequest(response),
                403 => StatusCode(403, response),
                404 => NotFound(response),
                409 => Conflict(response),
                _ => StatusCode(statusCode, response)
            };
        }
    }
}