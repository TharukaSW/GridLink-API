using GridLink.DTOs;
using GridLink.Helpers;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using GridLink.Services.Interfaces;

namespace GridLink.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<UserResponseDTO>> CreateUserAsync(UserRCreateDTO dto)
        {
            if (dto.Role != UserRole.BackOffice && dto.Role != UserRole.GridOperator)
            {
                return ApiResponse<UserResponseDTO>.FailureResult(
                    "Invalid role. Administrative creation is restricted to 'Backoffice' and 'GridOperator' roles.");
            }

            if (await _userRepository.ExistsByNicAsync(dto.NIC))
            {
                return ApiResponse<UserResponseDTO>.FailureResult(
                    "A user with this National Identity Card (NIC) already exists.");
            }

            if (await _userRepository.ExistsByEmailAsync(dto.Email))
            {
                return ApiResponse<UserResponseDTO>.FailureResult(
                    "A user with this Email address already exists.");
            }

            var newUser = new Users
            {
                NIC = dto.NIC.Trim().ToUpper(),
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLower(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Address = dto.Address?.Trim() ?? string.Empty,
                HashPassword = PasswordHasher.HashPassword(dto.Password),
                Role = dto.Role,
                Status = UserStatus.Active, // Admin-created staff accounts are immediately active
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(newUser);
            return ApiResponse<UserResponseDTO>.SuccessResult(
                MapToDto(newUser),
                $"{dto.Role} user created successfully.");
        }

        public async Task<ApiResponse<List<UserResponseDTO>>> GetAllUsersAsync(string? role = null, string? status = null)
        {
            var users = await _userRepository.GetAllAsync(role, status);
            var dtos = users.Select(MapToDto).ToList();
            return ApiResponse<List<UserResponseDTO>>.SuccessResult(dtos, $"Retrieved {dtos.Count} users.");
        }

        public async Task<ApiResponse<UserResponseDTO>> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponse<UserResponseDTO>.FailureResult("User not found.");
            }

            return ApiResponse<UserResponseDTO>.SuccessResult(MapToDto(user));
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateProfileAsync(string userId, UpdateProfileDTO dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserResponseDTO>.FailureResult("User not found.");
            }

            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = dto.PhoneNumber.Trim();
            user.Address = dto.Address?.Trim() ?? string.Empty;

            await _userRepository.UpdateAsync(user);
            return ApiResponse<UserResponseDTO>.SuccessResult(MapToDto(user), "Profile updated successfully.");
        }

        public async Task<ApiResponse<UserResponseDTO>> GetUserByNicAsync(string nic)
        {
            var user = await _userRepository.GetByNicAsync(nic);
            if (user == null)
            {
                return ApiResponse<UserResponseDTO>.FailureResult($"User with NIC '{nic}' not found.");
            }

            return ApiResponse<UserResponseDTO>.SuccessResult(MapToDto(user));
        }

        public async Task<ApiResponse<List<UserResponseDTO>>> GetPendingActivationsAsync()
        {
            var pendingUsers = await _userRepository.GetPendingActivationsAsync();
            var dtos = pendingUsers.Select(MapToDto).ToList();
            return ApiResponse<List<UserResponseDTO>>.SuccessResult(dtos, $"Retrieved {dtos.Count} pending activations.");
        }


        public async Task<ApiResponse<UserResponseDTO>> UpdateUserAsync(string id, UpdateUserDTO dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponse<UserResponseDTO>.FailureResult("User not found.");
            }

            // Check if updated email conflicts with another user
            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailOwner = await _userRepository.GetByEmailAsync(dto.Email);
                if (emailOwner != null && emailOwner.Id != user.Id)
                {
                    return ApiResponse<UserResponseDTO>.FailureResult("Email address is already in use by another user.");
                }
            }

            user.FullName = dto.FullName.Trim();
            user.Email = dto.Email.Trim().ToLower();
            user.PhoneNumber = dto.PhoneNumber.Trim();
            user.Address = dto.Address?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(dto.Role) && UserRole.AllRoles.Contains(dto.Role))
            {
                user.Role = dto.Role;
            }

            await _userRepository.UpdateAsync(user);
            return ApiResponse<UserResponseDTO>.SuccessResult(MapToDto(user), "User details updated successfully.");
        }

        public async Task<ApiResponse<UserResponseDTO>> ActivateUserAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponse<UserResponseDTO>.FailureResult("User not found.");
            }

            if (user.Status == UserStatus.Active)
            {
                return ApiResponse<UserResponseDTO>.FailureResult("Account is already active.");
            }

            user.Status = UserStatus.Active;
            await _userRepository.UpdateAsync(user);

            return ApiResponse<UserResponseDTO>.SuccessResult(MapToDto(user), "Account activated successfully.");
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.FailureResult("User not found.");
            }

            if (!PasswordHasher.VerifyPassword(dto.CurrentPassword, user.HashPassword))
            {
                return ApiResponse<bool>.FailureResult("Current password is incorrect.");
            }

            user.HashPassword = PasswordHasher.HashPassword(dto.NewPassword);
            await _userRepository.UpdateAsync(user);

            return ApiResponse<bool>.SuccessResult(true, "Password changed successfully.");
        }

        public async Task<ApiResponse<UserResponseDTO>> DeactivateUserAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponse<UserResponseDTO>.FailureResult("User not found.");
            }

            if (user.Status == UserStatus.Deactivated)
            {
                return ApiResponse<UserResponseDTO>.FailureResult("Account is already deactivated.");
            }

            user.Status = UserStatus.Deactivated;
            await _userRepository.UpdateAsync(user);

            return ApiResponse<UserResponseDTO>.SuccessResult(MapToDto(user), "Account deactivated successfully.");
        }



        public async Task<ApiResponse<bool>> DeleteUserAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponse<bool>.FailureResult("User not found.");
            }

            bool deleted = await _userRepository.DeleteAsync(id);
            if (!deleted)
            {
                return ApiResponse<bool>.FailureResult("Failed to delete user.");
            }

            return ApiResponse<bool>.SuccessResult(true, "User deleted successfully.");
        }

        private static UserResponseDTO MapToDto(Users user)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                NIC = user.NIC,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
