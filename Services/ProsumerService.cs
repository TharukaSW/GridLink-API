using GridLink.DTOs;
using GridLink.Helpers;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using GridLink.Services.Interfaces;

namespace GridLink.Services
{
    public class ProsumerService : IProsumerService
    {
        private readonly IProsumerRepository _prosumerRepo;

        public ProsumerService(IProsumerRepository prosumerRepo)
        {
            _prosumerRepo = prosumerRepo;
        }

        public async Task<(int StatusCode, string Message, ProsumerResponseDTO? Data)> RegisterAsync(ProsumerRegisterRequestDTO dto)
        {
            if (!NicValidator.IsValid(dto.NIC))
                return (400, "Invalid NIC format", null);

            var normalizedNic = NicValidator.Normalize(dto.NIC);

            if (await _prosumerRepo.GetByNicAsync(normalizedNic) != null)
                return (409, "NIC is already registered", null);

            if (await _prosumerRepo.GetByEmailAsync(dto.Email) != null)
                return (409, "Email is already in use", null);

            var prosumer = new Users
            {
                NIC = normalizedNic,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                HashPassword = PasswordHasher.HashPassword(dto.Password),
                Role = UserRole.Prosumer,
                Status = UserStatus.PendingActivation,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _prosumerRepo.CreateAsync(prosumer);

            return (201, "Prosumer registered successfully", MapToDto(prosumer));
        }

        public async Task<(int StatusCode, string Message, List<ProsumerResponseDTO>? Data)> GetAllAsync(string? status)
        {
            var users = await _prosumerRepo.GetAllProsumersAsync(status);
            var dtos = users.Select(MapToDto).ToList();
            return (200, "Prosumers retrieved successfully", dtos);
        }

        public async Task<(int StatusCode, string Message, ProsumerResponseDTO? Data)> GetByNicAsync(string targetNic, string? callerNic, string? callerRole)
        {
            if (!IsOwnerOrStaff(targetNic, callerNic, callerRole))
                return (403, "Access denied. You can only view your own profile.", null);

            var user = await _prosumerRepo.GetByNicAsync(NicValidator.Normalize(targetNic));
            if (user == null)
                return (404, "Prosumer not found", null);

            return (200, "Prosumer retrieved successfully", MapToDto(user));
        }

        public async Task<(int StatusCode, string Message, ProsumerResponseDTO? Data)> UpdateProfileAsync(string targetNic, ProsumerUpdateRequestDTO dto, string? callerNic, string? callerRole)
        {
            if (!IsOwnerOrStaff(targetNic, callerNic, callerRole))
                return (403, "Access denied. You can only update your own profile.", null);

            var user = await _prosumerRepo.GetByNicAsync(NicValidator.Normalize(targetNic));
            if (user == null)
                return (404, "Prosumer not found", null);

            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _prosumerRepo.GetByEmailAsync(dto.Email) != null)
                    return (409, "Email is already in use by another account", null);
            }

            await _prosumerRepo.UpdateProfileAsync(user.Id, dto.FullName, dto.Email, dto.PhoneNumber, dto.Address);

            var updatedUser = await _prosumerRepo.GetByNicAsync(NicValidator.Normalize(targetNic));
            return (200, "Profile updated successfully", MapToDto(updatedUser!));
        }

        public async Task<(int StatusCode, string Message)> RequestDeactivationAsync(string targetNic, string? callerNic, string? callerRole)
        {
            if (callerNic != NicValidator.Normalize(targetNic))
                return (403, "Access denied. You can only deactivate your own account.");

            var user = await _prosumerRepo.GetByNicAsync(NicValidator.Normalize(targetNic));
            if (user == null)
                return (404, "Prosumer not found");

            if (user.Status != UserStatus.Active)
                return (409, $"Cannot request deactivation. Current status is {user.Status}. Must be Active.");

            await _prosumerRepo.UpdateStatusAsync(user.Id, UserStatus.DeactivationRequested);
            return (200, "Deactivation requested successfully");
        }

        public async Task<(int StatusCode, string Message)> DeactivateAsync(string targetNic)
        {
            var user = await _prosumerRepo.GetByNicAsync(NicValidator.Normalize(targetNic));
            if (user == null)
                return (404, "Prosumer not found");

            if (user.Status != UserStatus.Active && user.Status != UserStatus.DeactivationRequested)
                return (409, $"Cannot deactivate. Current status is {user.Status}. Must be Active or DeactivationRequested.");

            await _prosumerRepo.UpdateStatusAsync(user.Id, UserStatus.Deactivated);
            return (200, "Prosumer deactivated successfully");
        }

        public async Task<(int StatusCode, string Message)> ActivateAsync(string targetNic)
        {
            var user = await _prosumerRepo.GetByNicAsync(NicValidator.Normalize(targetNic));
            if (user == null)
                return (404, "Prosumer not found");

            if (user.Status != UserStatus.PendingActivation && user.Status != UserStatus.Deactivated)
                return (409, $"Cannot activate. Current status is {user.Status}. Must be PendingActivation or Deactivated.");

            await _prosumerRepo.UpdateStatusAsync(user.Id, UserStatus.Active);
            return (200, "Prosumer activated successfully");
        }


        private bool IsOwnerOrStaff(string targetNic, string? callerNic, string? callerRole)
        {
            if (callerRole == UserRole.BackOffice || callerRole == UserRole.GridOperator)
                return true;

            return NicValidator.Normalize(targetNic) == callerNic;
        }

        private ProsumerResponseDTO MapToDto(Users user)
        {
            return new ProsumerResponseDTO
            {
                NIC = user.NIC ?? string.Empty,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = user.Address ?? string.Empty,
                Status = user.Status,
                CreatedAt = user.CreatedAt
            };
        }
    }
}