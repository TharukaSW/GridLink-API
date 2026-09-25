using GridLink.Data;
using GridLink.DTOs;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using MongoDB.Driver;

namespace GridLink.Repositories
{
    public class EnergyReservationRepository : IEnergyReservationRepository
    {
        private readonly IMongoCollection<EnergyReservation> _reservations;

        public EnergyReservationRepository(MongoDbContext context)
        {
            _reservations = context.energyReservations;
        }

        public async Task<EnergyReservation?> GetByIdAsync(string id)
        {
            return await _reservations.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task<EnergyReservation?> GetByReservationNumberAsync(string reservationNumber)
        {
            return await _reservations.Find(r => r.ReservationNumber.ToLower() == reservationNumber.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<List<EnergyReservation>> GetByUserIdAsync(string userId, string? status = null)
        {
            var filterBuilder = Builders<EnergyReservation>.Filter;
            var filter = filterBuilder.Eq(r => r.UserId, userId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                filter &= filterBuilder.Eq(r => r.Status, status);
            }

            return await _reservations.Find(filter)
                .SortByDescending(r => r.ReservationDateTime)
                .ToListAsync();
        }

        public async Task<List<EnergyReservation>> GetByNicAsync(string nic, string? status = null)
        {
            var filterBuilder = Builders<EnergyReservation>.Filter;
            var filter = filterBuilder.Eq(r => r.NIC, nic.Trim().ToUpper());

            if (!string.IsNullOrWhiteSpace(status))
            {
                filter &= filterBuilder.Eq(r => r.Status, status);
            }

            return await _reservations.Find(filter)
                .SortByDescending(r => r.ReservationDateTime)
                .ToListAsync();
        }

        public async Task<List<EnergyReservation>> GetAllAsync(ReservationFilterDTO? filter = null)
        {
            var filterBuilder = Builders<EnergyReservation>.Filter;
            var mongoFilter = filterBuilder.Empty;

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.NIC))
                {
                    mongoFilter &= filterBuilder.Eq(r => r.NIC, filter.NIC.Trim().ToUpper());
                }

                if (!string.IsNullOrWhiteSpace(filter.StationId))
                {
                    mongoFilter &= filterBuilder.Eq(r => r.StationId, filter.StationId);
                }

                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    mongoFilter &= filterBuilder.Eq(r => r.Status, filter.Status);
                }

                if (!string.IsNullOrWhiteSpace(filter.TradingType))
                {
                    mongoFilter &= filterBuilder.Eq(r => r.TradingType, filter.TradingType);
                }

                if (filter.FromDate.HasValue)
                {
                    mongoFilter &= filterBuilder.Gte(r => r.ReservationDateTime, filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    mongoFilter &= filterBuilder.Lte(r => r.ReservationDateTime, filter.ToDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    string term = filter.SearchTerm.Trim();
                    var searchFilter = filterBuilder.Or(
                        filterBuilder.Regex(r => r.ReservationNumber, new MongoDB.Bson.BsonRegularExpression(term, "i")),
                        filterBuilder.Regex(r => r.NIC, new MongoDB.Bson.BsonRegularExpression(term, "i")),
                        filterBuilder.Regex(r => r.ProsumerName, new MongoDB.Bson.BsonRegularExpression(term, "i")),
                        filterBuilder.Regex(r => r.StationName, new MongoDB.Bson.BsonRegularExpression(term, "i"))
                    );
                    mongoFilter &= searchFilter;
                }
            }

            return await _reservations.Find(mongoFilter)
                .SortByDescending(r => r.ReservationDateTime)
                .ToListAsync();
        }

        public async Task<long> CountByStatusAsync(string status, DateTime? futureOnlyAfter = null)
        {
            var filterBuilder = Builders<EnergyReservation>.Filter;
            var filter = filterBuilder.Eq(r => r.Status, status);

            if (futureOnlyAfter.HasValue)
            {
                filter &= filterBuilder.Gte(r => r.ReservationDateTime, futureOnlyAfter.Value);
            }

            return await _reservations.CountDocumentsAsync(filter);
        }

        public async Task<long> TotalCountAsync(string? nic = null)
        {
            if (string.IsNullOrWhiteSpace(nic))
            {
                return await _reservations.CountDocumentsAsync(Builders<EnergyReservation>.Filter.Empty);
            }

            return await _reservations.CountDocumentsAsync(r => r.NIC == nic.Trim().ToUpper());
        }

        public async Task<long> CountActiveAsync(DateTime fromDateTime, string? nic = null)
        {
            var filterBuilder = Builders<EnergyReservation>.Filter;
            var filter = filterBuilder.In(r => r.Status, new[] { ReservationStatus.Pending, ReservationStatus.Approved })
                         & filterBuilder.Gte(r => r.ReservationDateTime, fromDateTime);

            if (!string.IsNullOrWhiteSpace(nic))
            {
                filter &= filterBuilder.Eq(r => r.NIC, nic.Trim().ToUpper());
            }

            return await _reservations.CountDocumentsAsync(filter);
        }

        public async Task CreateAsync(EnergyReservation reservation)
        {
            await _reservations.InsertOneAsync(reservation);
        }

        public async Task<bool> UpdateAsync(EnergyReservation reservation)
        {
            reservation.UpdatedAt = DateTime.UtcNow;
            var result = await _reservations.ReplaceOneAsync(r => r.Id == reservation.Id, reservation);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _reservations.DeleteOneAsync(r => r.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> HasActiveReservationForStationAsync(string stationId)
        {
            var filterBuilder = Builders<EnergyReservation>.Filter;
            var filter = filterBuilder.Eq(r => r.StationId, stationId)
                         & filterBuilder.In(r => r.Status, new[] { ReservationStatus.Pending, ReservationStatus.Approved });

            return await _reservations.Find(filter).AnyAsync();
        }
    }
}
