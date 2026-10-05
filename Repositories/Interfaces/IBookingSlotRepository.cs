// ============================================================================
// File: IBookingSlotRepository.cs
// Project: GridLink - Smart Solar Microgrid Trading System
// Module: Microgrid Node Management API - Slot Booking Management
// Description: Contract for MongoDB repository operations for microgrid energy
//              booking slots. Supports slot querying, creation, modifications,
//              and cascading removals.
// Course: SE4040 Enterprise Application Development - Assignment 1
// ============================================================================

using GridLink.Models;

namespace GridLink.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for solar microgrid energy booking slots.
    /// </summary>
    public interface IBookingSlotRepository
    {
        /// <summary>
        /// Retrieves all booking slots for a given solar station, optionally filtered by availability.
        /// </summary>
        Task<List<EnergyBookingSlot>> GetByStationIdAsync(string stationId, bool? availableOnly = null);

        /// <summary>
        /// Retrieves a specific booking slot by its unique MongoDB identifier.
        /// </summary>
        Task<EnergyBookingSlot?> GetByIdAsync(string id);

        /// <summary>
        /// Inserts a new energy booking slot into the database.
        /// </summary>
        Task CreateAsync(EnergyBookingSlot slot);

        /// <summary>
        /// Updates an existing booking slot with modified details.
        /// </summary>
        Task<bool> UpdateAsync(EnergyBookingSlot slot);

        /// <summary>
        /// Permanently removes a booking slot by its identifier.
        /// </summary>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Deletes all booking slots belonging to a given station ID (cascade cleanup).
        /// </summary>
        Task<long> DeleteByStationIdAsync(string stationId);

        /// <summary>
        /// Checks if a booking slot exists with the specified identifier.
        /// </summary>
        Task<bool> ExistsAsync(string id);

        /// <summary>
        /// Updates the availability flag for a specific slot.
        /// </summary>
        Task<bool> SetAvailabilityAsync(string slotId, bool isAvailable);

        /// <summary>
        /// Calculates the next available slot number for a solar station.
        /// </summary>
        Task<int> GetNextSlotNumberAsync(string stationId);
    }
}
