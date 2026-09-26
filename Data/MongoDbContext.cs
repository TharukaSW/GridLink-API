using GridLink.Models;
using GridLink.Helpers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GridLink.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDBSetting _setting;

        public MongoDbContext (IOptions<MongoDBSetting> setting)
        {
            _setting = setting.Value;
            var client = new MongoClient (_setting.ConnectionString);
            _database = client.GetDatabase(_setting.DatabaseName);

            ConfigureIndexes();
            SeedInitialAdminUser();
        }

        public IMongoCollection<Users> users => _database.GetCollection<Users>(_setting.UserCollectioNames);
        
        public void ConfigureIndexes()
        {
            try
            {
                var emailIndexKeys = Builders<Users>.IndexKeys.Ascending(u => u.Email);
                var emailIndexOptions = new CreateIndexOptions { Unique = true };
                var emailIndexModel = new CreateIndexModel<Users>(emailIndexKeys, emailIndexOptions);

                var nicIndexKeys = Builders<Users>.IndexKeys.Ascending(u => u.NIC);
                var nicIndexOptions = new CreateIndexOptions 
                { 
                    Unique = true, 
                    Sparse = true,
                    PartialFilterExpression = Builders<Users>.Filter.Ne(u => u.NIC, string.Empty) 
                };
                var nicIndexModel = new CreateIndexModel<Users>(nicIndexKeys, nicIndexOptions);

                users.Indexes.CreateMany(new[] { nicIndexModel, emailIndexModel });
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[MongoDbContext] Notice: Index configuration skipped or already exists: {ex.Message}");
            }
        }

        private void SeedInitialAdminUser()
        {
            try
            {
                bool adminExists = users.Find(u => u.Role == UserRole.BackOffice).Any();
                if (!adminExists)
                {
                    var defaultAdmin = new Users
                    {
                        NIC = "ADMIN00001V",
                        FullName = "System Administrator",
                        Email = "admin@gridlink.com",
                        PhoneNumber = "0771234567",
                        Address = "GridLink Operations Center, Colombo",
                        Role = UserRole.BackOffice,
                        Status = UserStatus.Active,
                        HashPassword = PasswordHasher.HashPassword("Admin@123"),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    users.InsertOne(defaultAdmin);
                    Console.WriteLine("[MongoDbContext] Seeded default Backoffice administrator account (NIC: ADMIN00001V / admin@gridlink.com).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MongoDbContext] Seeding note: {ex.Message}");
            }
        }
    }
}
