using GridLink.Data;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using MongoDB.Driver;
using static QRCoder.PayloadGenerator.SwissQrCode;

namespace GridLink.Repositories
{
    public class UserRepository : IUserRepository
    {

        private readonly IMongoCollection<Users> _User;

        public UserRepository(MongoDbContext context)
        {
            _User = context.users;
        }

        public async Task<Users?> GetByIdAsync(string id)
        {
            return await _User.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Users?> GetUsersAsync(string id)
        {
            return await _User.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Users?> GetByNicAsync(string nic)
        {
            return await _User.Find(u => u.NIC.ToLower() == nic.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<Users?> GetByEmailAsync(string email)
        {
            return await _User.Find(u => u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<Users?> GetByIdentifierAsync(string identifier)
        {
            string cleanIdentifier = identifier.Trim().ToLower();
            return await _User.Find(u => u.NIC.ToLower() == cleanIdentifier || u.Email.ToLower() == cleanIdentifier)
                               .FirstOrDefaultAsync();
        }

        public async Task<List<Users>> GetAllAsync(string? role = null, string? status = null)
        {
            var filterBuilder = Builders<Users>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrWhiteSpace(role))
            {
                filter &= filterBuilder.Eq(u => u.Role, role);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filter &= filterBuilder.Eq(u => u.Status, status);
            }

            return await _User.Find(filter)
                               .SortByDescending(u => u.CreatedAt)
                               .ToListAsync();
        }

        public async Task<List<Users>> GetPendingActivationsAsync()
        {
            return await _User.Find(u => u.Status == UserStatus.PendingActivation)
                               .SortBy(u => u.CreatedAt)
                               .ToListAsync();
        }

        public async Task CreateAsync(Users user)
        {
            await _User.InsertOneAsync(user);
        }

        public async Task<bool> UpdateAsync(Users user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _User.ReplaceOneAsync(u => u.Id == user.Id, user);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _User.DeleteOneAsync(u => u.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> ExistsByNicAsync(string nic)
        {
            return await _User.Find(u => u.NIC.ToLower() == nic.ToLower()).AnyAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _User.Find(u => u.Email.ToLower() == email.ToLower()).AnyAsync();
        }


    }
}
