// ============================================================================
// File: MicrogridNodeService.cs
// Project: GridLink - Smart Solar Microgrid Trading System
// Module: Microgrid Node Management API
// Description: Core enterprise service implementing FAT Service pattern for
//              solar microgrid hub management. Contains all business logic,
//              enforcing rules such as deactivation blocking when active
//              reservations exist, GPS distance calculations for Google Maps,
//              battery slot availability, and slot management.
// Course: SE4040 Enterprise Application Development - Assignment 1
// ============================================================================

using GridLink.DTOs;
using GridLink.Helpers;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using GridLink.Services.Interfaces;

namespace GridLink.Services
{
    /// <summary>
    /// Service implementation for managing solar microgrid nodes and energy booking slots.
    /// </summary>
    public class MicrogridNodeService : IMicrogridNodeService
    {
        private readonly ISolarStationRepository _stationRepository;
        private readonly IBookingSlotRepository _slotRepository;
        private readonly IEnergyReservationRepository _reservationRepository;

        private const double EarthRadiusKm = 6371.0;

        /// <summary>
        /// Initializes a new instance of MicrogridNodeService with required repositories.
        /// </summary>
        /// <param name="stationRepository">Solar station repository</param>
        /// <param name="slotRepository">Energy booking slot repository</param>
        /// <param name="reservationRepository">Energy reservation repository</param>
        public MicrogridNodeService(
            ISolarStationRepository stationRepository,
            IBookingSlotRepository slotRepository,
            IEnergyReservationRepository reservationRepository)
        {
            _stationRepository = stationRepository;
            _slotRepository = slotRepository;
            _reservationRepository = reservationRepository;
        }

        /// <summary>
        /// Retrieves all microgrid nodes filtered by search criteria, status, and capacity.
        /// </summary>
        /// <param name="filter">Optional query filter parameters</param>
        /// <returns>API response containing list of microgrid node response DTOs</returns>
        public async Task<ApiResponse<List<MicrogridNodeResponseDTO>>> GetAllNodesAsync(NodeFilterDTO? filter)
        {
            // Query stations matching criteria from the repository
            var stations = await _stationRepository.GetAllAsync(
                activeOnly: filter?.IsActive,
                searchTerm: filter?.SearchTerm,
                minCapacity: filter?.MinCapacity,
                maxCapacity: filter?.MaxCapacity);

            var result = new List<MicrogridNodeResponseDTO>();
            foreach (var s in stations)
            {
                result.Add(MapToDto(s));
            }

            return ApiResponse<List<MicrogridNodeResponseDTO>>.SuccessResult(
                result,
                $"Successfully retrieved {result.Count} microgrid node(s).");
        }

        /// <summary>
        /// Retrieves a microgrid node by its unique MongoDB identifier.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <returns>API response containing the node details</returns>
        public async Task<ApiResponse<MicrogridNodeResponseDTO>> GetNodeByIdAsync(string id)
        {
            // Find station document by ID
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Microgrid node not found.");
            }

            return ApiResponse<MicrogridNodeResponseDTO>.SuccessResult(MapToDto(station));
        }

        /// <summary>
        /// Retrieves a microgrid node by its station code.
        /// </summary>
        /// <param name="stationCode">Station code string</param>
        /// <returns>API response containing the node details</returns>
        public async Task<ApiResponse<MicrogridNodeResponseDTO>> GetNodeByCodeAsync(string stationCode)
        {
            // Validate code parameter and find station
            if (string.IsNullOrWhiteSpace(stationCode))
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Station code cannot be empty.");
            }

            var station = await _stationRepository.GetByCodeAsync(stationCode);
            if (station == null)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult($"Microgrid node with code '{stationCode}' was not found.");
            }

            return ApiResponse<MicrogridNodeResponseDTO>.SuccessResult(MapToDto(station));
        }

        /// <summary>
        /// Registers a new solar microgrid hub, validating unique code and capacity specifications.
        /// </summary>
        /// <param name="dto">Creation payload</param>
        /// <returns>API response with created node details</returns>
        public async Task<ApiResponse<MicrogridNodeResponseDTO>> CreateNodeAsync(CreateMicrogridNodeDTO dto)
        {
            // Enforce unique StationCode rule
            string formattedCode = dto.StationCode.Trim().ToUpper();
            bool codeExists = await _stationRepository.ExistsByCodeAsync(formattedCode);
            if (codeExists)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult(
                    $"Station code '{formattedCode}' is already registered. Please choose a unique code.");
            }

            // Validate capacity and slots business logic
            if (dto.CapacityKWh <= 0)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Station capacity must be greater than 0 kW/h.");
            }

            if (dto.TotalBatterySlots <= 0)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Total battery slots must be at least 1.");
            }

            int availableSlots = dto.AvailableBatterySlots ?? dto.TotalBatterySlots;
            if (availableSlots > dto.TotalBatterySlots)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult(
                    "Available battery slots cannot exceed total battery slots.");
            }

            if (availableSlots < 0)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Available battery slots cannot be negative.");
            }

            // Construct new SolarStationInfo entity
            var station = new SolarStationInfo
            {
                StationCode = formattedCode,
                StationName = dto.StationName.Trim(),
                Address = dto.Address.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CapacityKWh = dto.CapacityKWh,
                TotalBatterySlots = dto.TotalBatterySlots,
                AvailableBatterySlots = availableSlots,
                OperatingHours = string.IsNullOrWhiteSpace(dto.OperatingHours) ? "06:00 - 22:00" : dto.OperatingHours.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _stationRepository.CreateAsync(station);

            return ApiResponse<MicrogridNodeResponseDTO>.SuccessResult(
                MapToDto(station),
                "Solar microgrid hub registered successfully.");
        }

        /// <summary>
        /// Updates solar microgrid node specifications and coordinates.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <param name="dto">Update payload</param>
        /// <returns>API response with updated node details</returns>
        public async Task<ApiResponse<MicrogridNodeResponseDTO>> UpdateNodeAsync(string id, UpdateMicrogridNodeDTO dto)
        {
            // Verify existing node existence
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Microgrid node not found.");
            }

            // Validate slot numbers
            if (dto.AvailableBatterySlots > dto.TotalBatterySlots)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult(
                    "Available battery slots cannot exceed total battery slots.");
            }

            if (dto.AvailableBatterySlots < 0)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Available battery slots cannot be negative.");
            }

            // Apply modifications
            station.StationName = dto.StationName.Trim();
            station.Address = dto.Address.Trim();
            station.Latitude = dto.Latitude;
            station.Longitude = dto.Longitude;
            station.CapacityKWh = dto.CapacityKWh;
            station.TotalBatterySlots = dto.TotalBatterySlots;
            station.AvailableBatterySlots = dto.AvailableBatterySlots;
            station.OperatingHours = dto.OperatingHours.Trim();
            station.IsActive = dto.IsActive;
            station.UpdatedAt = DateTime.UtcNow;

            bool updated = await _stationRepository.UpdateAsync(station);
            if (!updated)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Failed to update microgrid node details.");
            }

            return ApiResponse<MicrogridNodeResponseDTO>.SuccessResult(
                MapToDto(station),
                "Microgrid node updated successfully.");
        }

        /// <summary>
        /// Updates the operational schedule (hours) for a solar microgrid hub.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <param name="dto">Schedule payload</param>
        /// <returns>API response with updated node details</returns>
        public async Task<ApiResponse<MicrogridNodeResponseDTO>> UpdateScheduleAsync(string id, UpdateScheduleDTO dto)
        {
            // Verify station existence
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Microgrid node not found.");
            }

            if (string.IsNullOrWhiteSpace(dto.OperatingHours))
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Operating hours cannot be blank.");
            }

            // Update schedule through repository
            bool updated = await _stationRepository.UpdateScheduleAsync(id, dto.OperatingHours);
            if (!updated)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Failed to update operational schedule.");
            }

            station.OperatingHours = dto.OperatingHours.Trim();
            station.UpdatedAt = DateTime.UtcNow;

            return ApiResponse<MicrogridNodeResponseDTO>.SuccessResult(
                MapToDto(station),
                "Operational schedule updated successfully.");
        }

        /// <summary>
        /// Updates available battery storage slots (performed by Grid Operators and Backoffice).
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <param name="dto">Battery slots payload</param>
        /// <returns>API response with updated node details</returns>
        public async Task<ApiResponse<MicrogridNodeResponseDTO>> UpdateBatterySlotsAsync(string id, UpdateBatterySlotsDTO dto)
        {
            // Retrieve station and validate available slot boundary
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Microgrid node not found.");
            }

            if (dto.AvailableBatterySlots < 0)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Available battery slots cannot be negative.");
            }

            if (dto.AvailableBatterySlots > station.TotalBatterySlots)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult(
                    $"Available battery slots ({dto.AvailableBatterySlots}) cannot exceed total battery capacity ({station.TotalBatterySlots}).");
            }

            // Persist updated slot count
            bool updated = await _stationRepository.SetSlotAvailabilityAsync(id, dto.AvailableBatterySlots);
            if (!updated)
            {
                return ApiResponse<MicrogridNodeResponseDTO>.FailureResult("Failed to update battery slot availability.");
            }

            station.AvailableBatterySlots = dto.AvailableBatterySlots;
            station.UpdatedAt = DateTime.UtcNow;

            return ApiResponse<MicrogridNodeResponseDTO>.SuccessResult(
                MapToDto(station),
                "Battery slot availability updated successfully.");
        }

        /// <summary>
        /// Deactivates a solar microgrid node. Enforces business rule: BLOCKED if active energy reservations exist.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <returns>API response indicating success or blocking reason</returns>
        public async Task<ApiResponse<bool>> DeactivateNodeAsync(string id)
        {
            // Verify station existence
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
            {
                return ApiResponse<bool>.FailureResult("Microgrid node not found.");
            }

            if (!station.IsActive)
            {
                return ApiResponse<bool>.FailureResult("Microgrid node is already deactivated.");
            }

            // CRITICAL BUSINESS RULE: Block deactivation if active (Pending or Approved) energy reservations exist
            bool hasActiveReservations = await _reservationRepository.HasActiveReservationForStationAsync(id);
            if (hasActiveReservations)
            {
                return ApiResponse<bool>.FailureResult(
                    "Cannot deactivate microgrid hub because active energy reservations exist for this station. " +
                    "All pending and approved power trading bookings must be finalized or cancelled before deactivation.");
            }

            // Perform deactivation
            bool updated = await _stationRepository.SetActiveStatusAsync(id, false);
            if (!updated)
            {
                return ApiResponse<bool>.FailureResult("Failed to deactivate microgrid node.");
            }

            return ApiResponse<bool>.SuccessResult(true, "Microgrid hub has been deactivated successfully.");
        }

        /// <summary>
        /// Reactivates an inactive solar microgrid node.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <returns>API response indicating success</returns>
        public async Task<ApiResponse<bool>> ActivateNodeAsync(string id)
        {
            // Verify station existence
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
            {
                return ApiResponse<bool>.FailureResult("Microgrid node not found.");
            }

            if (station.IsActive)
            {
                return ApiResponse<bool>.FailureResult("Microgrid node is already active.");
            }

            // Perform reactivation
            bool updated = await _stationRepository.SetActiveStatusAsync(id, true);
            if (!updated)
            {
                return ApiResponse<bool>.FailureResult("Failed to reactivate microgrid node.");
            }

            return ApiResponse<bool>.SuccessResult(true, "Microgrid hub has been reactivated successfully.");
        }

        /// <summary>
        /// Deletes a solar microgrid node. Blocked if active energy reservations exist.
        /// </summary>
        /// <param name="id">Station identifier</param>
        /// <returns>API response indicating success or blocking reason</returns>
        public async Task<ApiResponse<bool>> DeleteNodeAsync(string id)
        {
            // Verify station existence
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
            {
                return ApiResponse<bool>.FailureResult("Microgrid node not found.");
            }

            // Block deletion if active energy reservations exist
            bool hasActiveReservations = await _reservationRepository.HasActiveReservationForStationAsync(id);
            if (hasActiveReservations)
            {
                return ApiResponse<bool>.FailureResult(
                    "Cannot delete microgrid hub because active energy reservations exist for this station. " +
                    "Please cancel or finalize all active reservations first, or deactivate the hub instead.");
            }

            // Remove associated booking slots cascade
            await _slotRepository.DeleteByStationIdAsync(id);

            // Delete station record
            bool deleted = await _stationRepository.DeleteAsync(id);
            if (!deleted)
            {
                return ApiResponse<bool>.FailureResult("Failed to delete microgrid node.");
            }

            return ApiResponse<bool>.SuccessResult(true, "Microgrid node and associated slots deleted successfully.");
        }

        /// <summary>
        /// Discovers nearby solar nodes within a geographic radius using the Haversine formula for map plotting.
        /// </summary>
        /// <param name="query">Proximity query parameters including lat, lng, radius</param>
        /// <returns>List of nearby nodes sorted by ascending distance with calculated distance</returns>
        public async Task<ApiResponse<List<MicrogridNodeResponseDTO>>> GetNearbyNodesAsync(NearbyNodeQueryDTO query)
        {
            // Fetch stations matching active constraint
            var stations = await _stationRepository.GetAllAsync(activeOnly: query.ActiveOnly ? true : null);

            var nearbyList = new List<MicrogridNodeResponseDTO>();
            foreach (var s in stations)
            {
                // Calculate distance using Haversine formula
                double distanceKm = CalculateHaversineDistanceKm(query.Latitude, query.Longitude, s.Latitude, s.Longitude);

                if (distanceKm <= query.RadiusKm)
                {
                    var dto = MapToDto(s);
                    dto.DistanceKm = Math.Round(distanceKm, 2);
                    nearbyList.Add(dto);
                }
            }

            // Sort by proximity: closest first
            nearbyList.Sort((a, b) => (a.DistanceKm ?? double.MaxValue).CompareTo(b.DistanceKm ?? double.MaxValue));

            return ApiResponse<List<MicrogridNodeResponseDTO>>.SuccessResult(
                nearbyList,
                $"Found {nearbyList.Count} microgrid node(s) within {query.RadiusKm} km.");
        }

        /// <summary>
        /// Retrieves all energy booking slots for a specified microgrid node.
        /// </summary>
        /// <param name="stationId">Station identifier</param>
        /// <param name="availableOnly">Filter for available slots only</param>
        /// <returns>List of booking slots</returns>
        public async Task<ApiResponse<List<BookingSlotResponseDTO>>> GetSlotsByStationIdAsync(string stationId, bool? availableOnly = null)
        {
            // Verify station existence
            bool stationExists = await _stationRepository.ExistsAsync(stationId);
            if (!stationExists)
            {
                return ApiResponse<List<BookingSlotResponseDTO>>.FailureResult("Microgrid station not found.");
            }

            var slots = await _slotRepository.GetByStationIdAsync(stationId, availableOnly);
            var dtos = slots.Select(MapSlotToDto).ToList();

            return ApiResponse<List<BookingSlotResponseDTO>>.SuccessResult(
                dtos,
                $"Retrieved {dtos.Count} booking slot(s).");
        }

        /// <summary>
        /// Retrieves a single energy booking slot by its identifier.
        /// </summary>
        /// <param name="slotId">Slot identifier</param>
        /// <returns>Booking slot response DTO</returns>
        public async Task<ApiResponse<BookingSlotResponseDTO>> GetSlotByIdAsync(string slotId)
        {
            // Fetch slot document
            var slot = await _slotRepository.GetByIdAsync(slotId);
            if (slot == null)
            {
                return ApiResponse<BookingSlotResponseDTO>.FailureResult("Booking slot not found.");
            }

            return ApiResponse<BookingSlotResponseDTO>.SuccessResult(MapSlotToDto(slot));
        }

        /// <summary>
        /// Creates a new energy booking slot for a microgrid station.
        /// </summary>
        /// <param name="dto">Slot creation payload</param>
        /// <returns>Created slot response DTO</returns>
        public async Task<ApiResponse<BookingSlotResponseDTO>> CreateSlotAsync(CreateSlotDTO dto)
        {
            // Validate station existence
            var station = await _stationRepository.GetByIdAsync(dto.StationId);
            if (station == null)
            {
                return ApiResponse<BookingSlotResponseDTO>.FailureResult("Microgrid station not found.");
            }

            if (!station.IsActive)
            {
                return ApiResponse<BookingSlotResponseDTO>.FailureResult("Cannot add booking slots to an inactive microgrid station.");
            }

            // Assign slot number
            int slotNumber = dto.SlotNumber ?? await _slotRepository.GetNextSlotNumberAsync(station.Id);

            var slot = new EnergyBookingSlot
            {
                StationId = station.Id,
                SlotNumber = slotNumber,
                SlotTime = dto.SlotTime.ToUniversalTime(),
                DurationMinutes = dto.DurationMinutes,
                CapacityKWh = dto.CapacityKWh > 0 ? dto.CapacityKWh : station.CapacityKWh / Math.Max(1, station.TotalBatterySlots),
                RatePerKWh = dto.RatePerKWh,
                IsAvailable = dto.IsAvailable,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _slotRepository.CreateAsync(slot);

            return ApiResponse<BookingSlotResponseDTO>.SuccessResult(
                MapSlotToDto(slot),
                "Energy booking slot created successfully.");
        }

        /// <summary>
        /// Updates an existing energy booking slot.
        /// </summary>
        /// <param name="slotId">Slot identifier</param>
        /// <param name="dto">Slot update payload</param>
        /// <returns>Updated slot response DTO</returns>
        public async Task<ApiResponse<BookingSlotResponseDTO>> UpdateSlotAsync(string slotId, UpdateSlotDTO dto)
        {
            // Fetch existing slot
            var slot = await _slotRepository.GetByIdAsync(slotId);
            if (slot == null)
            {
                return ApiResponse<BookingSlotResponseDTO>.FailureResult("Booking slot not found.");
            }

            // Apply updates
            slot.SlotTime = dto.SlotTime.ToUniversalTime();
            slot.DurationMinutes = dto.DurationMinutes;
            slot.CapacityKWh = dto.CapacityKWh;
            slot.RatePerKWh = dto.RatePerKWh;
            slot.IsAvailable = dto.IsAvailable;
            slot.UpdatedAt = DateTime.UtcNow;

            bool updated = await _slotRepository.UpdateAsync(slot);
            if (!updated)
            {
                return ApiResponse<BookingSlotResponseDTO>.FailureResult("Failed to update booking slot.");
            }

            return ApiResponse<BookingSlotResponseDTO>.SuccessResult(
                MapSlotToDto(slot),
                "Booking slot updated successfully.");
        }

        /// <summary>
        /// Deletes an energy booking slot. Blocked if actively reserved.
        /// </summary>
        /// <param name="slotId">Slot identifier</param>
        /// <returns>True if deleted</returns>
        public async Task<ApiResponse<bool>> DeleteSlotAsync(string slotId)
        {
            // Verify slot existence
            var slot = await _slotRepository.GetByIdAsync(slotId);
            if (slot == null)
            {
                return ApiResponse<bool>.FailureResult("Booking slot not found.");
            }

            // Perform deletion
            bool deleted = await _slotRepository.DeleteAsync(slotId);
            if (!deleted)
            {
                return ApiResponse<bool>.FailureResult("Failed to delete booking slot.");
            }

            return ApiResponse<bool>.SuccessResult(true, "Booking slot deleted successfully.");
        }

        /// <summary>
        /// Calculates the great-circle distance between two geographic coordinates using the Haversine formula.
        /// </summary>
        private static double CalculateHaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            return EarthRadiusKm * c;
        }

        /// <summary>
        /// Converts angle in degrees to radians.
        /// </summary>
        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        /// <summary>
        /// Maps a SolarStationInfo database entity to its response DTO.
        /// </summary>
        private static MicrogridNodeResponseDTO MapToDto(SolarStationInfo s)
        {
            return new MicrogridNodeResponseDTO
            {
                Id = s.Id,
                StationCode = s.StationCode,
                StationName = s.StationName,
                Address = s.Address,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                CapacityKWh = s.CapacityKWh,
                TotalBatterySlots = s.TotalBatterySlots,
                AvailableBatterySlots = s.AvailableBatterySlots,
                OperatingHours = s.OperatingHours,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            };
        }

        /// <summary>
        /// Maps an EnergyBookingSlot entity to its response DTO.
        /// </summary>
        private static BookingSlotResponseDTO MapSlotToDto(EnergyBookingSlot slot)
        {
            return new BookingSlotResponseDTO
            {
                Id = slot.Id,
                StationId = slot.StationId,
                SlotNumber = slot.SlotNumber,
                SlotTime = slot.SlotTime,
                DurationMinutes = slot.DurationMinutes,
                CapacityKWh = slot.CapacityKWh,
                RatePerKWh = slot.RatePerKWh,
                IsAvailable = slot.IsAvailable,
                CreatedAt = slot.CreatedAt,
                UpdatedAt = slot.UpdatedAt
            };
        }
    }
}
