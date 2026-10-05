// ============================================================================
// File: EnergyBookingSlot.cs
// Project: GridLink - Smart Solar Microgrid Trading System
// Module: Microgrid Node Management API - Slot Booking Management
// Description: MongoDB entity model for energy drop-off and charging booking
//              slots allocated under solar microgrid hubs.
// Course: SE4040 Enterprise Application Development - Assignment 1
// ============================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GridLink.Models
{
    /// <summary>
    /// Represents a schedulable energy booking slot allocated for a microgrid hub.
    /// </summary>
    public class EnergyBookingSlot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("stationId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string StationId { get; set; } = string.Empty;

        [BsonElement("slotNumber")]
        public int SlotNumber { get; set; }

        [BsonElement("slotTime")]
        public DateTime SlotTime { get; set; }

        [BsonElement("durationMinutes")]
        public int DurationMinutes { get; set; } = 60;

        [BsonElement("capacityKWh")]
        public double CapacityKWh { get; set; }

        [BsonElement("isAvailable")]
        public bool IsAvailable { get; set; } = true;

        [BsonElement("ratePerKWh")]
        public decimal RatePerKWh { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
