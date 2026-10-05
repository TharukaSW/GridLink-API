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
    public class ReservationsController : ControllerBase
    {
        private readonly IEnergyReservationService _reservationService;

        public ReservationsController(IEnergyReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRole.Prosumer},{UserRole.BackOffice}")]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ReservationResponseDTO>.FailureResult(
                    "Invalid reservation payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? currentNic = User.FindFirst("nic")?.Value;

            if (string.IsNullOrWhiteSpace(currentUserId) || string.IsNullOrWhiteSpace(currentNic))
            {
                return Unauthorized(ApiResponse<ReservationResponseDTO>.FailureResult("User details not found in authentication token."));
            }

            var result = await _reservationService.CreateReservationAsync(currentUserId, currentNic, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateReservation(string id, [FromBody] UpdateReservationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ReservationResponseDTO>.FailureResult(
                    "Invalid update payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(currentUserId) || string.IsNullOrWhiteSpace(currentUserRole))
            {
                return Unauthorized(ApiResponse<ReservationResponseDTO>.FailureResult("User not recognized from token."));
            }

            var result = await _reservationService.UpdateReservationAsync(id, currentUserId, currentUserRole, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelReservation(string id, [FromBody] CancelReservationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ReservationResponseDTO>.FailureResult(
                    "Invalid cancellation payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            string? currentNic = User.FindFirst("nic")?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(currentUserId) || string.IsNullOrWhiteSpace(currentUserRole))
            {
                return Unauthorized(ApiResponse<ReservationResponseDTO>.FailureResult("User not recognized from token."));
            }

            var result = await _reservationService.CancelReservationAsync(id, currentUserId, currentUserRole, currentNic, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = $"{UserRole.GridOperator},{UserRole.BackOffice}")]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveReservation(string id)
        {
            string? currentNic = User.FindFirst("nic")?.Value ?? "OPERATOR";

            var result = await _reservationService.ApproveReservationAsync(id, currentNic);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("verify")]
        [Authorize(Roles = $"{UserRole.GridOperator},{UserRole.BackOffice}")]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyQrCode([FromBody] string qrData)
        {
            if (string.IsNullOrWhiteSpace(qrData))
            {
                return BadRequest(ApiResponse<ReservationResponseDTO>.FailureResult("QR code payload cannot be empty."));
            }

            var result = await _reservationService.VerifyQrCodeAsync(qrData);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/finalize")]
        [Authorize(Roles = $"{UserRole.GridOperator},{UserRole.BackOffice}")]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FinalizeReservation(string id, [FromBody] FinalizeReservationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ReservationResponseDTO>.FailureResult(
                    "Invalid finalization payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            string? currentNic = User.FindFirst("nic")?.Value ?? "OPERATOR";

            var result = await _reservationService.FinalizeReservationAsync(id, currentNic, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("my-bookings")]
        [ProducesResponseType(typeof(ApiResponse<List<ReservationResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyReservations([FromQuery] string? status)
        {
            string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized(ApiResponse<List<ReservationResponseDTO>>.FailureResult("User not recognized from token."));
            }

            var result = await _reservationService.GetMyReservationsAsync(currentUserId, status);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReservationResponseDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            string? currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            var result = await _reservationService.GetByIdAsync(id, currentUserId, currentUserRole);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRole.GridOperator},{UserRole.BackOffice}")]
        [ProducesResponseType(typeof(ApiResponse<List<ReservationResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllReservations([FromQuery] ReservationFilterDTO filter)
        {
            var result = await _reservationService.GetAllReservationsAsync(filter);
            return Ok(result);
        }

        [HttpGet("dashboard/summary")]
        [ProducesResponseType(typeof(ApiResponse<ReservationDashboardSummaryDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] string? nic)
        {
            string? currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            string? currentNic = User.FindFirst("nic")?.Value;

            string? targetNic = currentUserRole == UserRole.Prosumer ? currentNic : nic;

            var result = await _reservationService.GetDashboardSummaryAsync(targetNic);
            return Ok(result);
        }

        [HttpGet("stations")]
        [ProducesResponseType(typeof(ApiResponse<List<SolarStationInfo>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableStations()
        {
            var result = await _reservationService.GetAvailableStationsAsync();
            return Ok(result);
        }
    }
}
