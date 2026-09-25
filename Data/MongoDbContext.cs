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
            SeedInitialSolarStations();
        }

        public IMongoCollection<Users> users => _database.GetCollection<Users>(_setting.UserCollectioNames);
        public IMongoCollection<SolarStationInfo> solarStations => _database.GetCollection<SolarStationInfo>(_setting.SolarStationCollectionName);
        public IMongoCollection<EnergyBookingSlot> energyBookingSlots => _database.GetCollection<EnergyBookingSlot>(_setting.EnergyBookingSlotCollectionName);
        public IMongoCollection<EnergyReservation> energyReservations => _database.GetCollection<EnergyReservation>(_setting.EnergyReservationCollectionName);
        
        public void ConfigureIndexes()
        {
            try
            {
                var indexKeysDefinition = Builders<Users>.IndexKeys.Ascending(u => u.NIC);
                var indexOptions = new CreateIndexOptions { Unique = true };
                var nicIndexModel = new CreateIndexModel<Users>(indexKeysDefinition, indexOptions);
                var emailIndexKeys = Builders<Users>.IndexKeys.Ascending(u => u.Email);
                var emailIndexModel = new CreateIndexModel<Users>(emailIndexKeys, indexOptions);

                users.Indexes.CreateMany(new[] { nicIndexModel, emailIndexModel });

                var stationCodeIndex = new CreateIndexModel<SolarStationInfo>(
                    Builders<SolarStationInfo>.IndexKeys.Ascending(s => s.StationCode),
                    new CreateIndexOptions { Unique = true });
                solarStations.Indexes.CreateOne(stationCodeIndex);

                var reservationNumberIndex = new CreateIndexModel<EnergyReservation>(
                    Builders<EnergyReservation>.IndexKeys.Ascending(r => r.ReservationNumber),
                    new CreateIndexOptions { Unique = true });
                var reservationNicIndex = new CreateIndexModel<EnergyReservation>(
                    Builders<EnergyReservation>.IndexKeys.Ascending(r => r.NIC));
                var reservationStationIndex = new CreateIndexModel<EnergyReservation>(
                    Builders<EnergyReservation>.IndexKeys.Ascending(r => r.StationId));
                var reservationDateIndex = new CreateIndexModel<EnergyReservation>(
                    Builders<EnergyReservation>.IndexKeys.Ascending(r => r.ReservationDateTime));
                var reservationStatusIndex = new CreateIndexModel<EnergyReservation>(
                    Builders<EnergyReservation>.IndexKeys.Ascending(r => r.Status));

                energyReservations.Indexes.CreateMany(new[]
                {
                    reservationNumberIndex,
                    reservationNicIndex,
                    reservationStationIndex,
                    reservationDateIndex,
                    reservationStatusIndex
                });

                var slotStationIndex = new CreateIndexModel<EnergyBookingSlot>(
                    Builders<EnergyBookingSlot>.IndexKeys.Ascending(s => s.StationId));
                var slotTimeIndex = new CreateIndexModel<EnergyBookingSlot>(
                    Builders<EnergyBookingSlot>.IndexKeys.Ascending(s => s.SlotTime));

                energyBookingSlots.Indexes.CreateMany(new[] { slotStationIndex, slotTimeIndex });
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[MongoDbContext] Notice: Index configuration skipped or already exists: {ex.Message}");
            }
        }

        private void SeedInitialSolarStations()
        {
            try
            {
                bool stationsExist = solarStations.Find(_ => true).Any();
                if (!stationsExist)
                {
                    var station1 = new SolarStationInfo
                    {
                        StationCode = "HUB-COL-01",
                        StationName = "Colombo Central Microgrid Hub",
                        Address = "No 45, Galle Road, Colombo 03",
                        Latitude = 6.9271,
                        Longitude = 79.8612,
                        CapacityKWh = 500.0,
                        TotalBatterySlots = 10,
                        AvailableBatterySlots = 8,
                        OperatingHours = "06:00 - 22:00",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var station2 = new SolarStationInfo
                    {
                        StationCode = "HUB-KDY-01",
                        StationName = "Kandy Hill Solar Hub",
                        Address = "Peradeniya Road, Kandy",
                        Latitude = 7.2906,
                        Longitude = 80.6337,
                        CapacityKWh = 350.0,
                        TotalBatterySlots = 8,
                        AvailableBatterySlots = 6,
                        OperatingHours = "06:00 - 20:00",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var station3 = new SolarStationInfo
                    {
                        StationCode = "HUB-GAL-01",
                        StationName = "Galle Coastal Solar Hub",
                        Address = "Matara Road, Galle",
                        Latitude = 6.0535,
                        Longitude = 80.2210,
                        CapacityKWh = 400.0,
                        TotalBatterySlots = 8,
                        AvailableBatterySlots = 7,
                        OperatingHours = "06:00 - 21:00",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    solarStations.InsertMany(new[] { station1, station2, station3 });

                    var now = DateTime.UtcNow;
                    var slots = new List<EnergyBookingSlot>();
                    for (int i = 1; i <= 5; i++)
                    {
                        slots.Add(new EnergyBookingSlot
                        {
                            StationId = station1.Id,
                            SlotNumber = i,
                            SlotTime = now.Date.AddDays(1).AddHours(8 + i * 2),
                            DurationMinutes = 60,
                            CapacityKWh = 50.0,
                            IsAvailable = true,
                            RatePerKWh = 45.0m,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                    }

                    energyBookingSlots.InsertMany(slots);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MongoDbContext] Station seeding note: {ex.Message}");
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
