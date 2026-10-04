// ============================================================================
// File: BookingSlotRepository.cs
// Project: GridLink - Smart Solar Microgrid Trading System
// Module: Microgrid Node Management API - Slot Booking Management
// Description: MongoDB repository implementation for energy booking slots.
//              Handles persistence, filtering by station, availability toggles,
//              and automatic slot indexing.
// Course: SE4040 Enterprise Application Development - Assignment 1
// ============================================================================

using GridLink.Data;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using MongoDB.Driver;

namespace GridLink.Repositories
{
    /// <summary>
    /// Repository handling MongoDB data access for solar microgrid energy booking slots.
    /// </summary>
    public class BookingSlotRepository : IBookingSlotRepository
    {
        private readonly IMongoCollection<EnergyBookingSlot> _slots;

        /// <summary>
        /// Initializes a new instance of BookingSlotRepository with the MongoDbContext.
        /// </summary>
        /// <param name="context">MongoDB application context</param>
        public BookingSlotRepository(MongoDbContext context)
        {
            // Resolve the energy booking slot collection from the context
            _slots = context.EnergyBookingSlots;
        }

        /// <summary>
        /// Retrieves all booking slots for a given solar station, optionally filtered by availability.
        /// </summary>
        /// <param name="stationId">Target station identifier</param>
        /// <param name="availableOnly">Filter for available slots only</param>
        /// <returns>List of matching booking slots sorted chronologically</returns>
        public async Task<List<EnergyBookingSlot>> GetByStationIdAsync(string stationId, bool? availableOnly = null)
        {
            // Build filter by station identifier
            var filterBuilder = Builders<EnergyBookingSlot>.Filter;
            var filter = filterBuilder.Eq(s => s.StationId, stationId);

            if (availableOnly.HasValue)
            {
                filter &= filterBuilder.Eq(s => s.IsAvailable, availableOnly.Value);
            }

            return await _slots.Find(filter)
                .SortBy(s => s.SlotTime)
                .ThenBy(s => s.SlotNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific booking slot by its unique MongoDB identifier.
        /// </summary>
        /// <param name="id">Slot ObjectId string</param>
        /// <returns>Slot document or null if not found</returns>
        public async Task<EnergyBookingSlot?> GetByIdAsync(string id)
        {
            // Find slot by primary identifier
            return await _slots.Find(s => s.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Inserts a new energy booking slot into the database.
        /// </summary>
        /// <param name="slot">Slot model to insert</param>
        public async Task CreateAsync(EnergyBookingSlot slot)
        {
            // Stamp timestamps and insert
            slot.CreatedAt = DateTime.UtcNow;
            slot.UpdatedAt = DateTime.UtcNow;
            await _slots.InsertOneAsync(slot);
        }

        /// <summary>
        /// Updates an existing booking slot with modified details.
        /// </summary>
        /// <param name="slot">Updated slot model</param>
        /// <returns>True if successfully updated</returns>
        public async Task<bool> UpdateAsync(EnergyBookingSlot slot)
        {
            // Update timestamp and replace document
            slot.UpdatedAt = DateTime.UtcNow;
            var result = await _slots.ReplaceOneAsync(s => s.Id == slot.Id, slot);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        /// <summary>
        /// Permanently removes a booking slot by its identifier.
        /// </summary>
        /// <param name="id">Slot ID</param>
        /// <returns>True if successfully deleted</returns>
        public async Task<bool> DeleteAsync(string id)
        {
            // Remove single slot document
            var result = await _slots.DeleteOneAsync(s => s.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        /// <summary>
        /// Deletes all booking slots belonging to a given station ID (cascade cleanup).
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <returns>Number of deleted slots</returns>
        public async Task<long> DeleteByStationIdAsync(string stationId)
        {
            // Remove all slots for this station
            var result = await _slots.DeleteManyAsync(s => s.StationId == stationId);
            return result.DeletedCount;
        }

        /// <summary>
        /// Checks if a booking slot exists with the specified identifier.
        /// </summary>
        /// <param name="id">Slot ID</param>
        /// <returns>True if slot exists</returns>
        public async Task<bool> ExistsAsync(string id)
        {
            // Query existence by ID
            return await _slots.Find(s => s.Id == id).AnyAsync();
        }

        /// <summary>
        /// Updates the availability flag for a specific slot.
        /// </summary>
        /// <param name="slotId">Slot ID</param>
        /// <param name="isAvailable">Availability status</param>
        /// <returns>True if updated successfully</returns>
        public async Task<bool> SetAvailabilityAsync(string slotId, bool isAvailable)
        {
            // Update slot availability and timestamp
            var update = Builders<EnergyBookingSlot>.Update
                .Set(s => s.IsAvailable, isAvailable)
                .Set(s => s.UpdatedAt, DateTime.UtcNow);

            var result = await _slots.UpdateOneAsync(s => s.Id == slotId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        /// <summary>
        /// Calculates the next available slot number for a solar station.
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <returns>Next sequential slot number starting from 1</returns>
        public async Task<int> GetNextSlotNumberAsync(string stationId)
        {
            // Retrieve latest slot number for the station
            var latest = await _slots.Find(s => s.StationId == stationId)
                .SortByDescending(s => s.SlotNumber)
                .FirstOrDefaultAsync();

            return latest == null ? 1 : latest.SlotNumber + 1;
        }
    }
}
