using GridLink.Data;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using MongoDB.Driver;

namespace GridLink.Repositories
{
    public class SolarStationRepository : ISolarStationRepository
    {
        private readonly IMongoCollection<SolarStationInfo> _stations;

        public SolarStationRepository(MongoDbContext context)
        {
            _stations = context.solarStations;
        }

        public async Task<SolarStationInfo?> GetByIdAsync(string id)
        {
            return await _stations.Find(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task<SolarStationInfo?> GetByCodeAsync(string stationCode)
        {
            return await _stations.Find(s => s.StationCode.ToLower() == stationCode.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<List<SolarStationInfo>> GetAllAsync(bool activeOnly = false)
        {
            if (activeOnly)
            {
                return await _stations.Find(s => s.IsActive).ToListAsync();
            }

            return await _stations.Find(_ => true).ToListAsync();
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _stations.Find(s => s.Id == id).AnyAsync();
        }

        public async Task<bool> UpdateSlotAvailabilityAsync(string stationId, int deltaAvailableSlots)
        {
            var update = Builders<SolarStationInfo>.Update
                .Inc(s => s.AvailableBatterySlots, deltaAvailableSlots)
                .Set(s => s.UpdatedAt, DateTime.UtcNow);

            var result = await _stations.UpdateOneAsync(s => s.Id == stationId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}
