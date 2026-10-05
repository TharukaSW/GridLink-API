using System.ComponentModel.DataAnnotations;

namespace GridLink.DTOs
{
    public class CreateReservationDTO
    {
        [Required(ErrorMessage = "Station ID is required")]
        public string StationId { get; set; } = string.Empty;

        public string? SlotId { get; set; }

        public int? SlotNumber { get; set; }

        [Required(ErrorMessage = "Reservation date and time is required")]
        public DateTime ReservationDateTime { get; set; }

        [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes")]
        public int DurationMinutes { get; set; } = 60;

        [Required(ErrorMessage = "Trading type is required (DropOff or Charging)")]
        public string TradingType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Requested energy amount is required")]
        [Range(0.1, 10000.0, ErrorMessage = "Requested energy must be between 0.1 kWh and 10,000 kWh")]
        public double RequestedEnergyKWh { get; set; }
    }

    public class UpdateReservationDTO
    {
        [Required(ErrorMessage = "Reservation date and time is required")]
        public DateTime ReservationDateTime { get; set; }

        [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes")]
        public int DurationMinutes { get; set; } = 60;

        [Required(ErrorMessage = "Trading type is required (DropOff or Charging)")]
        public string TradingType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Requested energy amount is required")]
        [Range(0.1, 10000.0, ErrorMessage = "Requested energy must be between 0.1 kWh and 10,000 kWh")]
        public double RequestedEnergyKWh { get; set; }

        public string? StationId { get; set; }
    }

    public class CancelReservationDTO
    {
        [Required(ErrorMessage = "Cancellation reason is required")]
        [StringLength(250, MinimumLength = 3, ErrorMessage = "Reason must be between 3 and 250 characters")]
        public string Reason { get; set; } = string.Empty;
    }

    public class FinalizeReservationDTO
    {
        [Required(ErrorMessage = "Actual energy transferred amount is required")]
        [Range(0.01, 10000.0, ErrorMessage = "Actual energy transferred must be greater than 0")]
        public double ActualEnergyTransferredKWh { get; set; }

        [StringLength(500, ErrorMessage = "Operator notes cannot exceed 500 characters")]
        public string? OperatorNotes { get; set; }
    }

    public class ReservationFilterDTO
    {
        public string? NIC { get; set; }
        public string? StationId { get; set; }
        public string? Status { get; set; }
        public string? TradingType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class ReservationResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string ReservationNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string NIC { get; set; } = string.Empty;
        public string ProsumerName { get; set; } = string.Empty;
        public string ProsumerContact { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string? SlotId { get; set; }
        public int? SlotNumber { get; set; }
        public DateTime ReservationDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public string TradingType { get; set; } = string.Empty;
        public double RequestedEnergyKWh { get; set; }
        public string Status { get; set; } = string.Empty;
        public string QRCodeData { get; set; } = string.Empty;
        public string QRCodeImageBase64 { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public string? FinalizedBy { get; set; }
        public double? ActualEnergyTransferredKWh { get; set; }
        public string? OperatorNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ReservationDashboardSummaryDTO
    {
        public long TotalReservations { get; set; }
        public long PendingCount { get; set; }
        public long ApprovedFutureCount { get; set; }
        public long CompletedCount { get; set; }
        public long CancelledCount { get; set; }
        public long ActiveCount { get; set; }
    }
}
