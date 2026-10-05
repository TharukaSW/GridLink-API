// ============================================================================
// File: SolarStationRepository.cs
// Project: GridLink - Smart Solar Microgrid Trading System
// Module: Microgrid Node Management API
// Description: MongoDB repository implementation for solar microgrid stations.
//              Handles persistence, indexing queries, atomic slot updates,
//              and status transitions for solar grid hubs.
// Course: SE4040 Enterprise Application Development - Assignment 1
// ============================================================================

using GridLink.Data;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GridLink.Repositories
{
    /// <summary>
    /// Repository handling MongoDB data access for solar microgrid hubs.
    /// </summary>
    public class SolarStationRepository : ISolarStationRepository
    {
        private readonly IMongoCollection<SolarStationInfo> _stations;

        /// <summary>
        /// Initializes a new instance of SolarStationRepository with the MongoDbContext.
        /// </summary>
        /// <param name="context">MongoDB application context</param>
        public SolarStationRepository(MongoDbContext context)
        {
            // Resolve the solar station collection from the database context
            _stations = context.SolarStations;
        }

        /// <summary>
        /// Retrieves a microgrid station by its unique MongoDB identifier.
        /// </summary>
        /// <param name="id">Station ObjectId string</param>
        /// <returns>Station document or null if not found</returns>
        public async Task<SolarStationInfo?> GetByIdAsync(string id)
        {
            // Find single station by matching primary ID
            return await _stations.Find(s => s.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a microgrid station by its station code (case-insensitive).
        /// </summary>
        /// <param name="stationCode">Unique station code (e.g. HUB-COL-01)</param>
        /// <returns>Station document or null if not found</returns>
        public async Task<SolarStationInfo?> GetByCodeAsync(string stationCode)
        {
            // Query station using regex case-insensitive match on StationCode
            var filter = Builders<SolarStationInfo>.Filter.Regex(
                s => s.StationCode,
                new BsonRegularExpression($"^{RegexEscape(stationCode.Trim())}$", "i"));

            return await _stations.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all microgrid stations with optional active status, search term, and capacity range filters.
        /// </summary>
        /// <param name="activeOnly">Filter by active status</param>
        /// <param name="searchTerm">Filter by code, name, or address</param>
        /// <param name="minCapacity">Minimum capacity in kW/h</param>
        /// <param name="maxCapacity">Maximum capacity in kW/h</param>
        /// <returns>List of matching solar stations</returns>
        public async Task<List<SolarStationInfo>> GetAllAsync(
            bool? activeOnly = null,
            string? searchTerm = null,
            double? minCapacity = null,
            double? maxCapacity = null)
        {
            // Build compound query filter dynamically
            var filterBuilder = Builders<SolarStationInfo>.Filter;
            var filter = filterBuilder.Empty;

            if (activeOnly.HasValue)
            {
                filter &= filterBuilder.Eq(s => s.IsActive, activeOnly.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim();
                var regex = new BsonRegularExpression(term, "i");
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(s => s.StationCode, regex),
                    filterBuilder.Regex(s => s.StationName, regex),
                    filterBuilder.Regex(s => s.Address, regex));
                filter &= searchFilter;
            }

            if (minCapacity.HasValue)
            {
                filter &= filterBuilder.Gte(s => s.CapacityKWh, minCapacity.Value);
            }

            if (maxCapacity.HasValue)
            {
                filter &= filterBuilder.Lte(s => s.CapacityKWh, maxCapacity.Value);
            }

            return await _stations.Find(filter)
                .SortBy(s => s.StationCode)
                .ToListAsync();
        }

        /// <summary>
        /// Overload to maintain backward compatibility with existing callers.
        /// </summary>
        /// <param name="activeOnly">Filter for active hubs only</param>
        /// <returns>List of matching solar stations</returns>
        public async Task<List<SolarStationInfo>> GetAllAsync(bool activeOnly)
        {
            // Delegate to the generalized filter method
            return await GetAllAsync(activeOnly: activeOnly, searchTerm: null, minCapacity: null, maxCapacity: null);
        }

        /// <summary>
        /// Checks whether a station exists with the given unique identifier.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <returns>True if station exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(string id)
        {
            // Perform existence query
            return await _stations.Find(s => s.Id == id).AnyAsync();
        }

        /// <summary>
        /// Checks whether a station code is already registered, optionally excluding a specific station ID.
        /// </summary>
        /// <param name="stationCode">Station code to check</param>
        /// <param name="excludeId">Optional ID to exclude from check (for updates)</param>
        /// <returns>True if code is taken, false otherwise</returns>
        public async Task<bool> ExistsByCodeAsync(string stationCode, string? excludeId = null)
        {
            // Build code filter case-insensitively
            var filterBuilder = Builders<SolarStationInfo>.Filter;
            var filter = filterBuilder.Regex(
                s => s.StationCode,
                new BsonRegularExpression($"^{RegexEscape(stationCode.Trim())}$", "i"));

            if (!string.IsNullOrWhiteSpace(excludeId))
            {
                filter &= filterBuilder.Ne(s => s.Id, excludeId);
            }

            return await _stations.Find(filter).AnyAsync();
        }

        /// <summary>
        /// Inserts a new solar microgrid station record into the database.
        /// </summary>
        /// <param name="station">Station model to persist</param>
        public async Task CreateAsync(SolarStationInfo station)
        {
            // Set timestamp metadata and insert document
            station.CreatedAt = DateTime.UtcNow;
            station.UpdatedAt = DateTime.UtcNow;
            await _stations.InsertOneAsync(station);
        }

        /// <summary>
        /// Replaces the existing solar station document with updated properties.
        /// </summary>
        /// <param name="station">Updated station model</param>
        /// <returns>True if modified count > 0</returns>
        public async Task<bool> UpdateAsync(SolarStationInfo station)
        {
            // Update timestamp and perform document replacement
            station.UpdatedAt = DateTime.UtcNow;
            var result = await _stations.ReplaceOneAsync(s => s.Id == station.Id, station);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        /// <summary>
        /// Permanently removes a solar microgrid station record by its identifier.
        /// </summary>
        /// <param name="id">Station ID to remove</param>
        /// <returns>True if deleted count > 0</returns>
        public async Task<bool> DeleteAsync(string id)
        {
            // Delete matching station document
            var result = await _stations.DeleteOneAsync(s => s.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        /// <summary>
        /// Increments or decrements the available battery slots for a station atomically.
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <param name="deltaAvailableSlots">Change in available slots (+1 or -1)</param>
        /// <returns>True if updated successfully</returns>
        public async Task<bool> UpdateSlotAvailabilityAsync(string stationId, int deltaAvailableSlots)
        {
            // Atomically update available battery slots and timestamp
            var update = Builders<SolarStationInfo>.Update
                .Inc(s => s.AvailableBatterySlots, deltaAvailableSlots)
                .Set(s => s.UpdatedAt, DateTime.UtcNow);

            var result = await _stations.UpdateOneAsync(s => s.Id == stationId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        /// <summary>
        /// Sets the exact number of available battery slots for a station.
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <param name="availableSlots">New count of available battery slots</param>
        /// <returns>True if updated successfully</returns>
        public async Task<bool> SetSlotAvailabilityAsync(string stationId, int availableSlots)
        {
            // Update available slots explicitly
            var update = Builders<SolarStationInfo>.Update
                .Set(s => s.AvailableBatterySlots, availableSlots)
                .Set(s => s.UpdatedAt, DateTime.UtcNow);

            var result = await _stations.UpdateOneAsync(s => s.Id == stationId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        /// <summary>
        /// Updates the operational schedule (operating hours) for a station.
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <param name="operatingHours">New operating hours string (e.g. 06:00 - 22:00)</param>
        /// <returns>True if updated successfully</returns>
        public async Task<bool> UpdateScheduleAsync(string stationId, string operatingHours)
        {
            // Update operating schedule hours and timestamp
            var update = Builders<SolarStationInfo>.Update
                .Set(s => s.OperatingHours, operatingHours.Trim())
                .Set(s => s.UpdatedAt, DateTime.UtcNow);

            var result = await _stations.UpdateOneAsync(s => s.Id == stationId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        /// <summary>
        /// Updates the active status flag of a microgrid station.
        /// </summary>
        /// <param name="id">Station ID</param>
        /// <param name="isActive">Active status value</param>
        /// <returns>True if updated successfully</returns>
        public async Task<bool> SetActiveStatusAsync(string id, bool isActive)
        {
            // Update active status flag and timestamp
            var update = Builders<SolarStationInfo>.Update
                .Set(s => s.IsActive, isActive)
                .Set(s => s.UpdatedAt, DateTime.UtcNow);

            var result = await _stations.UpdateOneAsync(s => s.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        /// <summary>
        /// Escapes regex special characters for safe regular expression querying.
        /// </summary>
        private static string RegexEscape(string input)
        {
            return System.Text.RegularExpressions.Regex.Escape(input);
        }
    }
}
