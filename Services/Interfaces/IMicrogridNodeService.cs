// ============================================================================
// File: IMicrogridNodeService.cs
// Project: GridLink - Smart Solar Microgrid Trading System
// Module: Microgrid Node Management API
// Description: Service contract defining enterprise business logic for solar
//              microgrid hubs and their booking slots, enforcing rules such
//              as reservation-blocked deactivation, schedule adjustments,
//              battery availability updates, and geospatial proximity calculations.
// Course: SE4040 Enterprise Application Development - Assignment 1
// ============================================================================

using GridLink.DTOs;
using GridLink.Helpers;

namespace GridLink.Services.Interfaces
{
    /// <summary>
    /// Service interface encapsulating business logic for solar microgrid hubs and slots.
    /// </summary>
    public interface IMicrogridNodeService
    {
        /// <summary>
        /// Retrieves all microgrid nodes filtered by search criteria, status, and capacity.
        /// </summary>
        Task<ApiResponse<List<MicrogridNodeResponseDTO>>> GetAllNodesAsync(NodeFilterDTO? filter);

        /// <summary>
        /// Retrieves a microgrid node by its unique MongoDB identifier.
        /// </summary>
        Task<ApiResponse<MicrogridNodeResponseDTO>> GetNodeByIdAsync(string id);

        /// <summary>
        /// Retrieves a microgrid node by its station code.
        /// </summary>
        Task<ApiResponse<MicrogridNodeResponseDTO>> GetNodeByCodeAsync(string stationCode);

        /// <summary>
        /// Registers a new solar microgrid hub, validating unique code and capacity specifications.
        /// </summary>
        Task<ApiResponse<MicrogridNodeResponseDTO>> CreateNodeAsync(CreateMicrogridNodeDTO dto);

        /// <summary>
        /// Updates solar microgrid node specifications and coordinates.
        /// </summary>
        Task<ApiResponse<MicrogridNodeResponseDTO>> UpdateNodeAsync(string id, UpdateMicrogridNodeDTO dto);

        /// <summary>
        /// Updates the operational schedule (hours) for a solar microgrid hub.
        /// </summary>
        Task<ApiResponse<MicrogridNodeResponseDTO>> UpdateScheduleAsync(string id, UpdateScheduleDTO dto);

        /// <summary>
        /// Updates available battery storage slots (performed by Grid Operators and Backoffice).
        /// </summary>
        Task<ApiResponse<MicrogridNodeResponseDTO>> UpdateBatterySlotsAsync(string id, UpdateBatterySlotsDTO dto);

        /// <summary>
        /// Deactivates a solar microgrid node. Enforces business rule: BLOCKED if active energy reservations exist.
        /// </summary>
        Task<ApiResponse<bool>> DeactivateNodeAsync(string id);

        /// <summary>
        /// Reactivates an inactive solar microgrid node.
        /// </summary>
        Task<ApiResponse<bool>> ActivateNodeAsync(string id);

        /// <summary>
        /// Deletes a solar microgrid node. Blocked if active energy reservations exist.
        /// </summary>
        Task<ApiResponse<bool>> DeleteNodeAsync(string id);

        /// <summary>
        /// Discovers nearby solar nodes within a geographic radius using the Haversine formula for map plotting.
        /// </summary>
        Task<ApiResponse<List<MicrogridNodeResponseDTO>>> GetNearbyNodesAsync(NearbyNodeQueryDTO query);

        /// <summary>
        /// Retrieves all energy booking slots for a specified microgrid node.
        /// </summary>
        Task<ApiResponse<List<BookingSlotResponseDTO>>> GetSlotsByStationIdAsync(string stationId, bool? availableOnly = null);

        /// <summary>
        /// Retrieves a single energy booking slot by its identifier.
        /// </summary>
        Task<ApiResponse<BookingSlotResponseDTO>> GetSlotByIdAsync(string slotId);

        /// <summary>
        /// Creates a new energy booking slot for a microgrid station.
        /// </summary>
        Task<ApiResponse<BookingSlotResponseDTO>> CreateSlotAsync(CreateSlotDTO dto);

        /// <summary>
        /// Updates an existing energy booking slot.
        /// </summary>
        Task<ApiResponse<BookingSlotResponseDTO>> UpdateSlotAsync(string slotId, UpdateSlotDTO dto);

        /// <summary>
        /// Deletes an energy booking slot. Blocked if actively reserved.
        /// </summary>
        Task<ApiResponse<bool>> DeleteSlotAsync(string slotId);
    }
}
