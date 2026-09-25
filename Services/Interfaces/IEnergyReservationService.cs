using GridLink.DTOs;
using GridLink.Helpers;
using GridLink.Models;

namespace GridLink.Services.Interfaces
{
    public interface IEnergyReservationService
    {
        Task<ApiResponse<ReservationResponseDTO>> CreateReservationAsync(string userId, string userNic, CreateReservationDTO dto);
        Task<ApiResponse<ReservationResponseDTO>> UpdateReservationAsync(string reservationId, string userId, string userRole, UpdateReservationDTO dto);
        Task<ApiResponse<ReservationResponseDTO>> CancelReservationAsync(string reservationId, string userId, string userRole, string userNic, CancelReservationDTO dto);
        Task<ApiResponse<ReservationResponseDTO>> ApproveReservationAsync(string reservationId, string operatorNic);
        Task<ApiResponse<ReservationResponseDTO>> FinalizeReservationAsync(string reservationId, string operatorNic, FinalizeReservationDTO dto);
        Task<ApiResponse<ReservationResponseDTO>> VerifyQrCodeAsync(string qrPayloadOrCode);
        Task<ApiResponse<ReservationResponseDTO>> GetByIdAsync(string reservationId, string userId, string userRole);
        Task<ApiResponse<ReservationResponseDTO>> GetByReservationNumberAsync(string reservationNumber, string userId, string userRole);
        Task<ApiResponse<List<ReservationResponseDTO>>> GetMyReservationsAsync(string userId, string? status = null);
        Task<ApiResponse<List<ReservationResponseDTO>>> GetAllReservationsAsync(ReservationFilterDTO filter);
        Task<ApiResponse<ReservationDashboardSummaryDTO>> GetDashboardSummaryAsync(string? nic = null);
        Task<ApiResponse<List<SolarStationInfo>>> GetAvailableStationsAsync();
    }
}
