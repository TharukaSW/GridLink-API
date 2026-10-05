using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GridLink.DTOs;
using GridLink.Helpers;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using GridLink.Services.Interfaces;

namespace GridLink.Services
{
    public class EnergyReservationService : IEnergyReservationService
    {
        private readonly IEnergyReservationRepository _reservationRepository;
        private readonly ISolarStationRepository _stationRepository;
        private readonly IUserRepository _userRepository;

        public EnergyReservationService(
            IEnergyReservationRepository reservationRepository,
            ISolarStationRepository stationRepository,
            IUserRepository userRepository)
        {
            _reservationRepository = reservationRepository;
            _stationRepository = stationRepository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<ReservationResponseDTO>> CreateReservationAsync(string userId, string userNic, CreateReservationDTO dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("User profile not found.");
            }

            if (user.Status != UserStatus.Active)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Your account is not active. Only active users can make reservations.");
            }

            var station = await _stationRepository.GetByIdAsync(dto.StationId);
            if (station == null)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Selected solar microgrid hub was not found.");
            }

            if (!station.IsActive)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Selected solar microgrid hub is currently inactive.");
            }

            if (dto.TradingType != TradingType.DropOff && dto.TradingType != TradingType.Charging)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Invalid trading type. Must be either 'DropOff' or 'Charging'.");
            }

            DateTime requestedUtc = dto.ReservationDateTime.ToUniversalTime();
            DateTime nowUtc = DateTime.UtcNow;

            if (requestedUtc < nowUtc)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservation date and time cannot be in the past.");
            }

            if (requestedUtc > nowUtc.AddDays(7))
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservations must be scheduled within 7 days from now.");
            }

            string reservationNumber = $"RES-{DateTime.UtcNow:yyyyMMdd}-{RandomNumberGenerator.GetInt32(10000, 99999)}";

            var reservation = new EnergyReservation
            {
                ReservationNumber = reservationNumber,
                UserId = user.Id,
                NIC = user.NIC,
                ProsumerName = user.FullName,
                ProsumerContact = user.PhoneNumber,
                StationId = station.Id,
                StationName = station.StationName,
                SlotId = dto.SlotId,
                SlotNumber = dto.SlotNumber ?? 1,
                ReservationDateTime = requestedUtc,
                DurationMinutes = dto.DurationMinutes,
                TradingType = dto.TradingType,
                RequestedEnergyKWh = dto.RequestedEnergyKWh,
                Status = ReservationStatus.Approved,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var qrPayload = BuildQrPayload(reservation);
            reservation.QRCodeData = qrPayload;
            reservation.QRCodeImageBase64 = QrCodeHelper.GenerateQrCodeBase64(qrPayload);

            await _reservationRepository.CreateAsync(reservation);

            if (station.AvailableBatterySlots > 0)
            {
                await _stationRepository.UpdateSlotAvailabilityAsync(station.Id, -1);
            }

            return ApiResponse<ReservationResponseDTO>.SuccessResult(
                MapToDto(reservation),
                "Energy slot reservation created and confirmed successfully.");
        }

        public async Task<ApiResponse<ReservationResponseDTO>> UpdateReservationAsync(
            string reservationId,
            string userId,
            string userRole,
            UpdateReservationDTO dto)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservation not found.");
            }

            if (userRole == UserRole.Prosumer && reservation.UserId != userId)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Access denied. You can only update your own reservations.");
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Cancelled reservations cannot be updated.");
            }

            if (reservation.Status == ReservationStatus.Completed)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Completed transactions cannot be modified.");
            }

            DateTime nowUtc = DateTime.UtcNow;
            if (reservation.ReservationDateTime - nowUtc < TimeSpan.FromHours(12))
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult(
                    "Updates require at least 12 hours' notice prior to the scheduled slot time.");
            }

            DateTime newSlotUtc = dto.ReservationDateTime.ToUniversalTime();
            if (newSlotUtc < nowUtc)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservation date and time cannot be in the past.");
            }

            if (newSlotUtc > nowUtc.AddDays(7))
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservations must be scheduled within 7 days from now.");
            }

            if (dto.TradingType != TradingType.DropOff && dto.TradingType != TradingType.Charging)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Invalid trading type. Must be either 'DropOff' or 'Charging'.");
            }

            if (!string.IsNullOrWhiteSpace(dto.StationId) && dto.StationId != reservation.StationId)
            {
                var newStation = await _stationRepository.GetByIdAsync(dto.StationId);
                if (newStation == null || !newStation.IsActive)
                {
                    return ApiResponse<ReservationResponseDTO>.FailureResult("Selected new solar hub is invalid or inactive.");
                }

                await _stationRepository.UpdateSlotAvailabilityAsync(reservation.StationId, 1);
                await _stationRepository.UpdateSlotAvailabilityAsync(newStation.Id, -1);

                reservation.StationId = newStation.Id;
                reservation.StationName = newStation.StationName;
            }

            reservation.ReservationDateTime = newSlotUtc;
            reservation.DurationMinutes = dto.DurationMinutes;
            reservation.TradingType = dto.TradingType;
            reservation.RequestedEnergyKWh = dto.RequestedEnergyKWh;
            reservation.UpdatedAt = DateTime.UtcNow;

            var qrPayload = BuildQrPayload(reservation);
            reservation.QRCodeData = qrPayload;
            reservation.QRCodeImageBase64 = QrCodeHelper.GenerateQrCodeBase64(qrPayload);

            await _reservationRepository.UpdateAsync(reservation);

            return ApiResponse<ReservationResponseDTO>.SuccessResult(
                MapToDto(reservation),
                "Reservation updated successfully.");
        }

        public async Task<ApiResponse<ReservationResponseDTO>> CancelReservationAsync(
            string reservationId,
            string userId,
            string userRole,
            string userNic,
            CancelReservationDTO dto)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservation not found.");
            }

            if (userRole == UserRole.Prosumer && reservation.UserId != userId)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Access denied. You can only cancel your own reservations.");
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservation is already cancelled.");
            }

            if (reservation.Status == ReservationStatus.Completed)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Completed transactions cannot be cancelled.");
            }

            DateTime nowUtc = DateTime.UtcNow;
            if (reservation.ReservationDateTime - nowUtc < TimeSpan.FromHours(12))
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult(
                    "Cancellations require at least 12 hours' notice prior to the scheduled slot time.");
            }

            reservation.Status = ReservationStatus.Cancelled;
            reservation.CancellationReason = dto.Reason.Trim();
            reservation.CancelledAt = DateTime.UtcNow;
            reservation.CancelledBy = userRole == UserRole.Prosumer ? "Prosumer" : $"{userRole}:{userNic}";
            reservation.UpdatedAt = DateTime.UtcNow;

            await _reservationRepository.UpdateAsync(reservation);
            await _stationRepository.UpdateSlotAvailabilityAsync(reservation.StationId, 1);

            return ApiResponse<ReservationResponseDTO>.SuccessResult(
                MapToDto(reservation),
                "Reservation cancelled successfully.");
        }

        public async Task<ApiResponse<ReservationResponseDTO>> ApproveReservationAsync(string reservationId, string operatorNic)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservation not found.");
            }

            if (reservation.Status == ReservationStatus.Approved)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservation is already approved.");
            }

            if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Completed)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult($"Cannot approve reservation with status '{reservation.Status}'.");
            }

            reservation.Status = ReservationStatus.Approved;
            reservation.UpdatedAt = DateTime.UtcNow;

            var qrPayload = BuildQrPayload(reservation);
            reservation.QRCodeData = qrPayload;
            reservation.QRCodeImageBase64 = QrCodeHelper.GenerateQrCodeBase64(qrPayload);

            await _reservationRepository.UpdateAsync(reservation);

            return ApiResponse<ReservationResponseDTO>.SuccessResult(
                MapToDto(reservation),
                "Reservation approved and QR code dispatched successfully.");
        }

        public async Task<ApiResponse<ReservationResponseDTO>> FinalizeReservationAsync(
            string reservationId,
            string operatorNic,
            FinalizeReservationDTO dto)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservation not found.");
            }

            if (reservation.Status == ReservationStatus.Completed)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("This energy transfer has already been finalized.");
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Cannot finalize a cancelled reservation.");
            }

            if (reservation.Status != ReservationStatus.Approved)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Only approved reservations can be finalized by an operator.");
            }

            reservation.Status = ReservationStatus.Completed;
            reservation.ActualEnergyTransferredKWh = dto.ActualEnergyTransferredKWh;
            reservation.OperatorNotes = dto.OperatorNotes?.Trim();
            reservation.FinalizedAt = DateTime.UtcNow;
            reservation.FinalizedBy = operatorNic;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _reservationRepository.UpdateAsync(reservation);
            await _stationRepository.UpdateSlotAvailabilityAsync(reservation.StationId, 1);

            return ApiResponse<ReservationResponseDTO>.SuccessResult(
                MapToDto(reservation),
                "Energy transfer finalized and recorded as completed.");
        }

        public async Task<ApiResponse<ReservationResponseDTO>> VerifyQrCodeAsync(string qrPayloadOrCode)
        {
            if (string.IsNullOrWhiteSpace(qrPayloadOrCode))
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("QR code payload is required.");
            }

            string cleanInput = qrPayloadOrCode.Trim();

            var reservation = await _reservationRepository.GetByReservationNumberAsync(cleanInput);
            if (reservation == null)
            {
                reservation = await _reservationRepository.GetByIdAsync(cleanInput);
            }

            if (reservation == null)
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(cleanInput);
                    if (jsonDoc.RootElement.TryGetProperty("resNum", out var resNumElement))
                    {
                        string resNum = resNumElement.GetString() ?? string.Empty;
                        reservation = await _reservationRepository.GetByReservationNumberAsync(resNum);
                    }
                    else if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
                    {
                        string id = idElement.GetString() ?? string.Empty;
                        reservation = await _reservationRepository.GetByIdAsync(id);
                    }
                }
                catch
                {
                }
            }

            if (reservation == null)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Invalid or unrecognized transaction QR code.");
            }

            return ApiResponse<ReservationResponseDTO>.SuccessResult(
                MapToDto(reservation),
                "QR code verified successfully. Reservation details loaded.");
        }

        public async Task<ApiResponse<ReservationResponseDTO>> GetByIdAsync(string reservationId, string userId, string userRole)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservation not found.");
            }

            if (userRole == UserRole.Prosumer && reservation.UserId != userId)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Access denied. You can only view your own reservation details.");
            }

            return ApiResponse<ReservationResponseDTO>.SuccessResult(MapToDto(reservation));
        }

        public async Task<ApiResponse<ReservationResponseDTO>> GetByReservationNumberAsync(string reservationNumber, string userId, string userRole)
        {
            var reservation = await _reservationRepository.GetByReservationNumberAsync(reservationNumber);
            if (reservation == null)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Reservation not found.");
            }

            if (userRole == UserRole.Prosumer && reservation.UserId != userId)
            {
                return ApiResponse<ReservationResponseDTO>.FailureResult("Access denied.");
            }

            return ApiResponse<ReservationResponseDTO>.SuccessResult(MapToDto(reservation));
        }

        public async Task<ApiResponse<List<ReservationResponseDTO>>> GetMyReservationsAsync(string userId, string? status = null)
        {
            var reservations = await _reservationRepository.GetByUserIdAsync(userId, status);
            var dtos = reservations.Select(MapToDto).ToList();
            return ApiResponse<List<ReservationResponseDTO>>.SuccessResult(
                dtos,
                $"Retrieved {dtos.Count} reservations.");
        }

        public async Task<ApiResponse<List<ReservationResponseDTO>>> GetAllReservationsAsync(ReservationFilterDTO filter)
        {
            var reservations = await _reservationRepository.GetAllAsync(filter);
            var dtos = reservations.Select(MapToDto).ToList();
            return ApiResponse<List<ReservationResponseDTO>>.SuccessResult(
                dtos,
                $"Retrieved {dtos.Count} reservations.");
        }

        public async Task<ApiResponse<ReservationDashboardSummaryDTO>> GetDashboardSummaryAsync(string? nic = null)
        {
            DateTime nowUtc = DateTime.UtcNow;

            long total = await _reservationRepository.TotalCountAsync(nic);
            long pending = await _reservationRepository.CountByStatusAsync(ReservationStatus.Pending);
            long approvedFuture = await _reservationRepository.CountByStatusAsync(ReservationStatus.Approved, nowUtc);
            long completed = await _reservationRepository.CountByStatusAsync(ReservationStatus.Completed);
            long cancelled = await _reservationRepository.CountByStatusAsync(ReservationStatus.Cancelled);
            long active = await _reservationRepository.CountActiveAsync(nowUtc, nic);

            var summary = new ReservationDashboardSummaryDTO
            {
                TotalReservations = total,
                PendingCount = pending,
                ApprovedFutureCount = approvedFuture,
                CompletedCount = completed,
                CancelledCount = cancelled,
                ActiveCount = active
            };

            return ApiResponse<ReservationDashboardSummaryDTO>.SuccessResult(summary, "Dashboard metrics retrieved successfully.");
        }

        public async Task<ApiResponse<List<SolarStationInfo>>> GetAvailableStationsAsync()
        {
            var stations = await _stationRepository.GetAllAsync(activeOnly: true);
            return ApiResponse<List<SolarStationInfo>>.SuccessResult(stations, $"Retrieved {stations.Count} solar stations.");
        }

        private static string BuildQrPayload(EnergyReservation r)
        {
            var payloadObj = new
            {
                resNum = r.ReservationNumber,
                nic = r.NIC,
                prosumer = r.ProsumerName,
                station = r.StationName,
                stationId = r.StationId,
                slotTime = r.ReservationDateTime.ToString("o"),
                duration = r.DurationMinutes,
                type = r.TradingType,
                kwh = r.RequestedEnergyKWh,
                status = r.Status,
                sec = ComputeSha256Checksum($"{r.ReservationNumber}:{r.NIC}:{r.ReservationDateTime:o}")
            };

            return JsonSerializer.Serialize(payloadObj);
        }

        private static string ComputeSha256Checksum(string rawData)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            for (int i = 0; i < Math.Min(bytes.Length, 8); i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        private static ReservationResponseDTO MapToDto(EnergyReservation r)
        {
            return new ReservationResponseDTO
            {
                Id = r.Id,
                ReservationNumber = r.ReservationNumber,
                UserId = r.UserId,
                NIC = r.NIC,
                ProsumerName = r.ProsumerName,
                ProsumerContact = r.ProsumerContact,
                StationId = r.StationId,
                StationName = r.StationName,
                SlotId = r.SlotId,
                SlotNumber = r.SlotNumber,
                ReservationDateTime = r.ReservationDateTime,
                DurationMinutes = r.DurationMinutes,
                TradingType = r.TradingType,
                RequestedEnergyKWh = r.RequestedEnergyKWh,
                Status = r.Status,
                QRCodeData = r.QRCodeData,
                QRCodeImageBase64 = r.QRCodeImageBase64,
                CancellationReason = r.CancellationReason,
                CancelledAt = r.CancelledAt,
                CancelledBy = r.CancelledBy,
                FinalizedAt = r.FinalizedAt,
                FinalizedBy = r.FinalizedBy,
                ActualEnergyTransferredKWh = r.ActualEnergyTransferredKWh,
                OperatorNotes = r.OperatorNotes,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            };
        }
    }
}
