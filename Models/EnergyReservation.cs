using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GridLink.Models
{
    public class EnergyReservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("reservationNumber")]
        public string ReservationNumber { get; set; } = string.Empty;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("nic")]
        public string NIC { get; set; } = string.Empty;

        [BsonElement("prosumerName")]
        public string ProsumerName { get; set; } = string.Empty;

        [BsonElement("prosumerContact")]
        public string ProsumerContact { get; set; } = string.Empty;

        [BsonElement("stationId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string StationId { get; set; } = string.Empty;

        [BsonElement("stationName")]
        public string StationName { get; set; } = string.Empty;

        [BsonElement("slotId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? SlotId { get; set; }

        [BsonElement("slotNumber")]
        public int? SlotNumber { get; set; }

        [BsonElement("reservationDateTime")]
        public DateTime ReservationDateTime { get; set; }

        [BsonElement("durationMinutes")]
        public int DurationMinutes { get; set; } = 60;

        [BsonElement("tradingType")]
        public string TradingType { get; set; } = Models.TradingType.DropOff;

        [BsonElement("requestedEnergyKWh")]
        public double RequestedEnergyKWh { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = ReservationStatus.Approved;

        [BsonElement("qrCodeData")]
        public string QRCodeData { get; set; } = string.Empty;

        [BsonElement("qrCodeImageBase64")]
        public string QRCodeImageBase64 { get; set; } = string.Empty;

        [BsonElement("cancellationReason")]
        public string? CancellationReason { get; set; }

        [BsonElement("cancelledAt")]
        public DateTime? CancelledAt { get; set; }

        [BsonElement("cancelledBy")]
        public string? CancelledBy { get; set; }

        [BsonElement("finalizedAt")]
        public DateTime? FinalizedAt { get; set; }

        [BsonElement("finalizedBy")]
        public string? FinalizedBy { get; set; }

        [BsonElement("actualEnergyTransferredKWh")]
        public double? ActualEnergyTransferredKWh { get; set; }

        [BsonElement("operatorNotes")]
        public string? OperatorNotes { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
