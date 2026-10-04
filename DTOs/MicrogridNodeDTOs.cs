// ============================================================================
// File: MicrogridNodeDTOs.cs
// Project: GridLink - Smart Solar Microgrid Trading System
// Module: Microgrid Node Management API
// Description: Data Transfer Objects (DTOs) for microgrid station hub creation,
//              updates, operational schedules, battery slot availability,
//              geospatial queries, and booking slot management.
// Course: SE4040 Enterprise Application Development - Assignment 1
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace GridLink.DTOs
{
    /// <summary>
    /// Request payload for creating a new solar microgrid hub.
    /// </summary>
    public class CreateMicrogridNodeDTO
    {
        [Required(ErrorMessage = "Station code is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Station code must be between 3 and 30 characters.")]
        public string StationCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Station name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Station name must be between 3 and 100 characters.")]
        public string StationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 250 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Latitude is required.")]
        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90.0 and 90.0 degrees.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180.0 and 180.0 degrees.")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Capacity specification (kW/h) is required.")]
        [Range(1.0, 100000.0, ErrorMessage = "Capacity must be greater than 0 kW/h.")]
        public double CapacityKWh { get; set; }

        [Required(ErrorMessage = "Total battery slots count is required.")]
        [Range(1, 500, ErrorMessage = "Total battery slots must be at least 1.")]
        public int TotalBatterySlots { get; set; }

        [Range(0, 500, ErrorMessage = "Available battery slots cannot be negative.")]
        public int? AvailableBatterySlots { get; set; }

        [StringLength(50, ErrorMessage = "Operating hours cannot exceed 50 characters.")]
        public string OperatingHours { get; set; } = "06:00 - 22:00";
    }

    /// <summary>
    /// Request payload for updating an existing solar microgrid hub.
    /// </summary>
    public class UpdateMicrogridNodeDTO
    {
        [Required(ErrorMessage = "Station name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Station name must be between 3 and 100 characters.")]
        public string StationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 250 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Latitude is required.")]
        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90.0 and 90.0 degrees.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180.0 and 180.0 degrees.")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Capacity specification (kW/h) is required.")]
        [Range(1.0, 100000.0, ErrorMessage = "Capacity must be greater than 0 kW/h.")]
        public double CapacityKWh { get; set; }

        [Required(ErrorMessage = "Total battery slots count is required.")]
        [Range(1, 500, ErrorMessage = "Total battery slots must be at least 1.")]
        public int TotalBatterySlots { get; set; }

        [Required(ErrorMessage = "Available battery slots is required.")]
        [Range(0, 500, ErrorMessage = "Available battery slots must be non-negative.")]
        public int AvailableBatterySlots { get; set; }

        [Required(ErrorMessage = "Operating hours schedule is required.")]
        [StringLength(50, ErrorMessage = "Operating hours cannot exceed 50 characters.")]
        public string OperatingHours { get; set; } = "06:00 - 22:00";

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Request payload for updating only the operational schedule of a solar hub.
    /// </summary>
    public class UpdateScheduleDTO
    {
        [Required(ErrorMessage = "Operating hours schedule is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Operating hours must be between 3 and 50 characters.")]
        public string OperatingHours { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request payload for Grid Operators updating available battery storage slots.
    /// </summary>
    public class UpdateBatterySlotsDTO
    {
        [Required(ErrorMessage = "Available battery slots count is required.")]
        [Range(0, 500, ErrorMessage = "Available battery slots must be between 0 and 500.")]
        public int AvailableBatterySlots { get; set; }
    }

    /// <summary>
    /// Query filter parameters for retrieving microgrid stations.
    /// </summary>
    public class NodeFilterDTO
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public double? MinCapacity { get; set; }
        public double? MaxCapacity { get; set; }
    }

    /// <summary>
    /// Query parameters for discovering nearby solar hubs for Google Maps.
    /// </summary>
    public class NearbyNodeQueryDTO
    {
        [Required(ErrorMessage = "Latitude is required.")]
        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90.0 and 90.0.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180.0 and 180.0.")]
        public double Longitude { get; set; }

        [Range(0.1, 500.0, ErrorMessage = "Radius must be between 0.1 km and 500 km.")]
        public double RadiusKm { get; set; } = 50.0;

        public bool ActiveOnly { get; set; } = true;
    }

    /// <summary>
    /// Comprehensive response representation of a solar microgrid node.
    /// </summary>
    public class MicrogridNodeResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double CapacityKWh { get; set; }
        public int TotalBatterySlots { get; set; }
        public int AvailableBatterySlots { get; set; }
        public string OperatingHours { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public double? DistanceKm { get; set; }
        public int ActiveReservationCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request payload for creating a new energy booking slot for a solar station.
    /// </summary>
    public class CreateSlotDTO
    {
        [Required(ErrorMessage = "Station ID is required.")]
        public string StationId { get; set; } = string.Empty;

        public int? SlotNumber { get; set; }

        [Required(ErrorMessage = "Slot time is required.")]
        public DateTime SlotTime { get; set; }

        [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes.")]
        public int DurationMinutes { get; set; } = 60;

        [Range(0.1, 10000.0, ErrorMessage = "Capacity must be positive.")]
        public double CapacityKWh { get; set; }

        [Range(0.01, 1000.0, ErrorMessage = "Rate per kWh must be greater than zero.")]
        public decimal RatePerKWh { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    /// <summary>
    /// Request payload for updating an existing energy booking slot.
    /// </summary>
    public class UpdateSlotDTO
    {
        public DateTime SlotTime { get; set; }

        [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes.")]
        public int DurationMinutes { get; set; } = 60;

        [Range(0.1, 10000.0, ErrorMessage = "Capacity must be positive.")]
        public double CapacityKWh { get; set; }

        [Range(0.01, 1000.0, ErrorMessage = "Rate per kWh must be greater than zero.")]
        public decimal RatePerKWh { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    /// <summary>
    /// Response representation for an energy booking slot.
    /// </summary>
    public class BookingSlotResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public int SlotNumber { get; set; }
        public DateTime SlotTime { get; set; }
        public int DurationMinutes { get; set; }
        public double CapacityKWh { get; set; }
        public decimal RatePerKWh { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
