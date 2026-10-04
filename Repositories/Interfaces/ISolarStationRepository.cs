// ============================================================================
// File: ISolarStationRepository.cs
// Project: GridLink - Smart Solar Microgrid Trading System
// Module: Microgrid Node Management API
// Description: Contract for solar microgrid station/node repository operations
//              including retrieval, existence checks, creation, updates, and
//              slot capacity management.
// Course: SE4040 Enterprise Application Development - Assignment 1
// ============================================================================

using GridLink.Models;

namespace GridLink.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for solar microgrid stations and nodes.
    /// </summary>
    public interface ISolarStationRepository
    {
        /// <summary>
        /// Retrieves a microgrid station by its unique MongoDB identifier.
        /// </summary>
        Task<SolarStationInfo?> GetByIdAsync(string id);

        /// <summary>
        /// Retrieves a microgrid station by its station code (case-insensitive).
        /// </summary>
        Task<SolarStationInfo?> GetByCodeAsync(string stationCode);

        /// <summary>
        /// Retrieves all microgrid stations with optional filtering by active status, search term, and capacity range.
        /// </summary>
        Task<List<SolarStationInfo>> GetAllAsync(bool? activeOnly = null, string? searchTerm = null, double? minCapacity = null, double? maxCapacity = null);

        /// <summary>
        /// Overload to maintain backward compatibility with existing callers.
        /// </summary>
        Task<List<SolarStationInfo>> GetAllAsync(bool activeOnly);

        /// <summary>
        /// Checks whether a station exists with the given unique identifier.
        /// </summary>
        Task<bool> ExistsAsync(string id);

        /// <summary>
        /// Checks whether a station code is already registered, optionally excluding a specific station ID.
        /// </summary>
        Task<bool> ExistsByCodeAsync(string stationCode, string? excludeId = null);

        /// <summary>
        /// Inserts a new solar microgrid station record into the database.
        /// </summary>
        Task CreateAsync(SolarStationInfo station);

        /// <summary>
        /// Replaces the existing solar station document with updated properties.
        /// </summary>
        Task<bool> UpdateAsync(SolarStationInfo station);

        /// <summary>
        /// Permanently removes a solar microgrid station record by its identifier.
        /// </summary>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Increments or decrements the available battery slots for a station atomically.
        /// </summary>
        Task<bool> UpdateSlotAvailabilityAsync(string stationId, int deltaAvailableSlots);

        /// <summary>
        /// Sets the exact number of available battery slots for a station.
        /// </summary>
        Task<bool> SetSlotAvailabilityAsync(string stationId, int availableSlots);

        /// <summary>
        /// Updates the operational schedule (operating hours) for a station.
        /// </summary>
        Task<bool> UpdateScheduleAsync(string stationId, string operatingHours);

        /// <summary>
        /// Updates the active status flag of a microgrid station.
        /// </summary>
        Task<bool> SetActiveStatusAsync(string id, bool isActive);
    }
}
