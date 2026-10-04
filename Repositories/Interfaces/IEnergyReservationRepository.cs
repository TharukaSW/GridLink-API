using GridLink.DTOs;
using GridLink.Models;

namespace GridLink.Repositories.Interfaces
{
    public interface IEnergyReservationRepository
    {
        Task<EnergyReservation?> GetByIdAsync(string id);
        Task<EnergyReservation?> GetByReservationNumberAsync(string reservationNumber);
        Task<List<EnergyReservation>> GetByUserIdAsync(string userId, string? status = null);
        Task<List<EnergyReservation>> GetByNicAsync(string nic, string? status = null);
        Task<List<EnergyReservation>> GetAllAsync(ReservationFilterDTO? filter = null);
        Task<long> CountByStatusAsync(string status, DateTime? futureOnlyAfter = null);
        Task<long> TotalCountAsync(string? nic = null);
        Task<long> CountActiveAsync(DateTime fromDateTime, string? nic = null);
        Task CreateAsync(EnergyReservation reservation);
        Task<bool> UpdateAsync(EnergyReservation reservation);
        Task<bool> DeleteAsync(string id);
        Task<bool> HasActiveReservationForStationAsync(string stationId);
    }
}
