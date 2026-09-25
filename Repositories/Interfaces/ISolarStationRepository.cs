using GridLink.Models;

namespace GridLink.Repositories.Interfaces
{
    public interface ISolarStationRepository
    {
        Task<SolarStationInfo?> GetByIdAsync(string id);
        Task<SolarStationInfo?> GetByCodeAsync(string stationCode);
        Task<List<SolarStationInfo>> GetAllAsync(bool activeOnly = false);
        Task<bool> ExistsAsync(string id);
        Task<bool> UpdateSlotAvailabilityAsync(string stationId, int deltaAvailableSlots);
    }
}
