// ============================================================================
// File: MicrogridNodesController.cs
// Project: GridLink - Smart Solar Microgrid Trading System
// Module: Microgrid Node Management API
// Description: REST Controller exposing endpoints for solar microgrid hub
//              registration, operational schedules, battery slot availability,
//              deactivation/activation status (with active reservation guard),
//              geospatial Google Maps queries, and energy booking slot operations.
// Course: SE4040 Enterprise Application Development - Assignment 1
// ============================================================================

using GridLink.DTOs;
using GridLink.Helpers;
using GridLink.Models;
using GridLink.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GridLink.Controllers
{
    /// <summary>
    /// Controller managing solar microgrid stations (nodes) and their booking slots.
    /// Accessible via /api/microgrid-nodes and /api/stations.
    /// </summary>
    [ApiController]
    [Route("api/microgrid-nodes")]
    [Route("api/stations")]
    [Authorize]
    public class MicrogridNodesController : ControllerBase
    {
        private readonly IMicrogridNodeService _nodeService;

        /// <summary>
        /// Initializes a new instance of MicrogridNodesController.
        /// </summary>
        /// <param name="nodeService">Microgrid node service</param>
        public MicrogridNodesController(IMicrogridNodeService nodeService)
        {
            // Inject business service dependency
            _nodeService = nodeService;
        }

        /// <summary>
        /// Retrieves all microgrid nodes matching optional filter parameters.
        /// </summary>
        /// <param name="filter">Optional query filters (search term, active status, capacity)</param>
        /// <returns>HTTP 200 with list of solar microgrid stations</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<MicrogridNodeResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllNodes([FromQuery] NodeFilterDTO? filter)
        {
            // Fetch list of microgrid nodes via service layer
            var result = await _nodeService.GetAllNodesAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Discovers nearby solar microgrid hubs based on GPS latitude, longitude, and radius.
        /// Used by the mobile and web clients for Google Maps visualization.
        /// </summary>
        /// <param name="query">Proximity query containing latitude, longitude, and radius in km</param>
        /// <returns>HTTP 200 with nearby hubs sorted by ascending distance</returns>
        [HttpGet("nearby")]
        [ProducesResponseType(typeof(ApiResponse<List<MicrogridNodeResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<MicrogridNodeResponseDTO>>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNearbyNodes([FromQuery] NearbyNodeQueryDTO query)
        {
            // Validate incoming query model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<List<MicrogridNodeResponseDTO>>.FailureResult(
                    "Invalid coordinates or radius supplied.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _nodeService.GetNearbyNodesAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves detailed information for a single microgrid node by its ID.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <returns>HTTP 200 with node details or 404 if not found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNodeById(string id)
        {
            // Fetch single node by primary ID
            var result = await _nodeService.GetNodeByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a microgrid node by its unique station code (e.g. HUB-COL-01).
        /// </summary>
        /// <param name="stationCode">Station code string</param>
        /// <returns>HTTP 200 with node details or 404 if not found</returns>
        [HttpGet("code/{stationCode}")]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNodeByCode(string stationCode)
        {
            // Fetch station by case-insensitive unique code
            var result = await _nodeService.GetNodeByCodeAsync(stationCode);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Registers a new solar microgrid hub. Restricted to Backoffice administrators.
        /// </summary>
        /// <param name="dto">Hub registration payload</param>
        /// <returns>HTTP 200 with created hub details or 400 on validation failure</returns>
        [HttpPost]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateNode([FromBody] CreateMicrogridNodeDTO dto)
        {
            // Validate incoming payload
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<MicrogridNodeResponseDTO>.FailureResult(
                    "Invalid microgrid hub payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _nodeService.CreateNodeAsync(dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing solar microgrid hub's information. Restricted to Backoffice administrators.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <param name="dto">Update payload</param>
        /// <returns>HTTP 200 with updated hub details or 400 on error</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateNode(string id, [FromBody] UpdateMicrogridNodeDTO dto)
        {
            // Validate update payload
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<MicrogridNodeResponseDTO>.FailureResult(
                    "Invalid update payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _nodeService.UpdateNodeAsync(id, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates the operational schedule (hours) for a solar microgrid hub. Restricted to Backoffice.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <param name="dto">Schedule payload</param>
        /// <returns>HTTP 200 with updated hub details or 400 on error</returns>
        [HttpPatch("{id}/schedule")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSchedule(string id, [FromBody] UpdateScheduleDTO dto)
        {
            // Validate schedule payload
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<MicrogridNodeResponseDTO>.FailureResult(
                    "Invalid schedule payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _nodeService.UpdateScheduleAsync(id, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates available battery storage slots. Accessible to both Grid Operators and Backoffice officers.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <param name="dto">Battery slots payload</param>
        /// <returns>HTTP 200 with updated hub details or 400 on error</returns>
        [HttpPatch("{id}/battery-slots")]
        [Authorize(Roles = $"{UserRole.GridOperator},{UserRole.BackOffice}")]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MicrogridNodeResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBatterySlots(string id, [FromBody] UpdateBatterySlotsDTO dto)
        {
            // Validate slots payload
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<MicrogridNodeResponseDTO>.FailureResult(
                    "Invalid battery slot payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _nodeService.UpdateBatterySlotsAsync(id, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Deactivates a solar microgrid node. Enforces business rule: BLOCKED if active energy reservations exist.
        /// Restricted to Backoffice administrators.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <returns>HTTP 200 on success or 400 if blocked by active reservations</returns>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeactivateNode(string id)
        {
            // Execute deactivation with reservation check
            var result = await _nodeService.DeactivateNodeAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Reactivates an inactive solar microgrid node. Restricted to Backoffice administrators.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <returns>HTTP 200 on success or 400 on failure</returns>
        [HttpPatch("{id}/activate")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActivateNode(string id)
        {
            // Execute reactivation
            var result = await _nodeService.ActivateNodeAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Permanently removes a microgrid node and associated slots. Blocked if active reservations exist.
        /// Restricted to Backoffice administrators.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <returns>HTTP 200 on success or 400 if blocked</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteNode(string id)
        {
            // Execute deletion with reservation safety guard
            var result = await _nodeService.DeleteNodeAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all energy booking slots for a specified solar microgrid hub.
        /// </summary>
        /// <param name="stationId">Station identifier</param>
        /// <param name="availableOnly">Filter for available slots only</param>
        /// <returns>HTTP 200 with list of booking slots</returns>
        [HttpGet("{stationId}/slots")]
        [ProducesResponseType(typeof(ApiResponse<List<BookingSlotResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSlotsByStation(string stationId, [FromQuery] bool? availableOnly)
        {
            // Query station slots
            var result = await _nodeService.GetSlotsByStationIdAsync(stationId, availableOnly);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a single energy booking slot by its ID.
        /// </summary>
        /// <param name="slotId">Slot identifier</param>
        /// <returns>HTTP 200 with slot details or 404 if not found</returns>
        [HttpGet("slots/{slotId}")]
        [ProducesResponseType(typeof(ApiResponse<BookingSlotResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BookingSlotResponseDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSlotById(string slotId)
        {
            // Retrieve single slot
            var result = await _nodeService.GetSlotByIdAsync(slotId);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Creates a new energy booking slot for a solar station. Accessible to Grid Operators and Backoffice.
        /// </summary>
        /// <param name="stationId">Target station identifier</param>
        /// <param name="dto">Slot creation payload</param>
        /// <returns>HTTP 200 with created slot details or 400 on error</returns>
        [HttpPost("{stationId}/slots")]
        [Authorize(Roles = $"{UserRole.GridOperator},{UserRole.BackOffice}")]
        [ProducesResponseType(typeof(ApiResponse<BookingSlotResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BookingSlotResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSlot(string stationId, [FromBody] CreateSlotDTO dto)
        {
            // Ensure route station ID matches payload
            dto.StationId = stationId;

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<BookingSlotResponseDTO>.FailureResult(
                    "Invalid slot payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _nodeService.CreateSlotAsync(dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing energy booking slot. Accessible to Grid Operators and Backoffice.
        /// </summary>
        /// <param name="slotId">Slot identifier</param>
        /// <param name="dto">Slot update payload</param>
        /// <returns>HTTP 200 with updated slot details or 400 on error</returns>
        [HttpPut("slots/{slotId}")]
        [Authorize(Roles = $"{UserRole.GridOperator},{UserRole.BackOffice}")]
        [ProducesResponseType(typeof(ApiResponse<BookingSlotResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BookingSlotResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSlot(string slotId, [FromBody] UpdateSlotDTO dto)
        {
            // Validate slot update payload
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<BookingSlotResponseDTO>.FailureResult(
                    "Invalid slot update payload.",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _nodeService.UpdateSlotAsync(slotId, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes an energy booking slot. Restricted to Backoffice administrators.
        /// </summary>
        /// <param name="slotId">Slot identifier</param>
        /// <returns>HTTP 200 on success or 400 on failure</returns>
        [HttpDelete("slots/{slotId}")]
        [Authorize(Roles = UserRole.BackOffice)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteSlot(string slotId)
        {
            // Execute slot removal
            var result = await _nodeService.DeleteSlotAsync(slotId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
