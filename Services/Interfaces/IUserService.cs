using GridLink.DTOs;
using GridLink.Helpers;

namespace GridLink.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserResponseDTO>> CreateUserAsync(UserRCreateDTO dto);
        Task<ApiResponse<List<UserResponseDTO>>> GetAllUsersAsync(string? role = null, string? status = null);
        Task<ApiResponse<UserResponseDTO>> GetUserByIdAsync(string id);
        Task<ApiResponse<UserResponseDTO>> GetUserByNicAsync(string nic);
        Task<ApiResponse<List<UserResponseDTO>>> GetPendingActivationsAsync();
        Task<ApiResponse<UserResponseDTO>> UpdateUserAsync(string id, UpdateUserDTO dto);
        Task<ApiResponse<UserResponseDTO>> ActivateUserAsync(string id);
        Task<ApiResponse<UserResponseDTO>> DeactivateUserAsync(string id);
        //Task<ApiResponse<bool>> RequestDeactivationAsync(string userId);
        Task<ApiResponse<UserResponseDTO>> UpdateProfileAsync(string userId, UpdateProfileDTO dto);
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<ApiResponse<bool>> DeleteUserAsync(string id);

    }
}
