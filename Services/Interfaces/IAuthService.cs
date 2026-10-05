using GridLink.DTOs;
using GridLink.Helpers;
using static GridLink.DTOs.AuthDTOs;

namespace GridLink.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<UserResponseDTO>> RegisterProsumerAsync(RegisterProsumerDto dto);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDTO dto);

    }
}
