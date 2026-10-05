using GridLink.Data;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using MongoDB.Driver;

namespace GridLink.Repositories
{
    public class ProsumerRepository : IProsumerRepository
    {
        private readonly IMongoCollection<Users> _users;

        public ProsumerRepository(MongoDbContext context)
        {

            _users = context.Users; 
        }

        public async Task<Users?> GetByNicAsync(string nic)
        {
            return await _users.Find(u => u.NIC == nic && u.Role == UserRole.Prosumer).FirstOrDefaultAsync();
        }

        public async Task<Users?> GetByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email && u.Role == UserRole.Prosumer).FirstOrDefaultAsync();
        }

        public async Task<List<Users>> GetAllProsumersAsync(string? status = null)
        {
            var filterBuilder = Builders<Users>.Filter;
            var filter = filterBuilder.Eq(u => u.Role, UserRole.Prosumer);

            if (!string.IsNullOrWhiteSpace(status))
            {
                filter &= filterBuilder.Eq(u => u.Status, status);
            }

            return await _users.Find(filter).ToListAsync();
        }

        public async Task CreateAsync(Users prosumer)
        {
            prosumer.Role = UserRole.Prosumer; 
            await _users.InsertOneAsync(prosumer);
        }

        public async Task<bool> UpdateProfileAsync(string id, string fullName, string email, string phone, string address)
        {
            var update = Builders<Users>.Update
                .Set(u => u.FullName, fullName)
                .Set(u => u.Email, email)
                .Set(u => u.PhoneNumber, phone)
                .Set(u => u.Address, address)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(
                u => u.Id == id && u.Role == UserRole.Prosumer, 
                update
            );
            
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateStatusAsync(string id, string status)
        {
            var update = Builders<Users>.Update
                .Set(u => u.Status, status)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(
                u => u.Id == id && u.Role == UserRole.Prosumer, 
                update
            );

            return result.ModifiedCount > 0;
        }
    }
}