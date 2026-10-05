using GridLink.Models;

namespace GridLink.Repositories.Interfaces
{
    public interface IProsumerRepository
    {
        Task<Users?> GetByNicAsync(string nic);
        Task <Users?> GetByEmailAsync(string email);
        Task<List<Users>> GetAllProsumersAsync (string? status = null);
        Task CreateAsync (Users prosumer);
        Task<bool> UpdateProfileAsync(string id, string fullName, string email, string phone, string address);
        Task<bool> UpdateStatusAsync(string id, string status);
    }
}