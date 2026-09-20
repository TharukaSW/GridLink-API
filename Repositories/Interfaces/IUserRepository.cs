using GridLink.Models;

namespace GridLink.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<Users?> GetByIdAsync (string id);
        Task<Users?> GetByNicAsync(string NIC);
        Task<Users?> GetByEmailAsync(string Email);
        Task<Users?> GetByIdentifierAsync(string Identifier);
        Task<List<Users>> GetAllAsync(string? role = null, string? status = null);
        Task<List<Users>> GetPendingActivationsAsync();
        Task CreateAsync(Users user);
        Task<bool> UpdateAsync(Users user);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsByNicAsync(string nic);
        Task<bool> ExistsByEmailAsync(string email);
    }
}
