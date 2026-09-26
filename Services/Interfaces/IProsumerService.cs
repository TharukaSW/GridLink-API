using GridLink.DTOs;

namespace GridLink.Services.Interfaces
{
    public interface IProsumerService
    {
        Task<(int StatusCode, string Message, ProsumerResponseDTO? Data)> RegisterAsync(ProsumerRegisterRequestDTO dto);
        Task<(int StatusCode, string Message, List<ProsumerResponseDTO>? Data)> GetAllAsync(string? status);
        Task<(int StatusCode, string Message, ProsumerResponseDTO? Data)> GetByNicAsync(string targetNic, string? callerNic, string? callerRole);
        Task<(int StatusCode, string Message, ProsumerResponseDTO? Data)> UpdateProfileAsync(string targetNic, ProsumerUpdateRequestDTO dto, string? callerNic, string? callerRole);
        Task<(int StatusCode, string Message)> RequestDeactivationAsync(string targetNic, string? callerNic, string? callerRole);
        Task<(int StatusCode, string Message)> DeactivateAsync(string targetNic);
        Task<(int StatusCode, string Message)> ActivateAsync(string targetNic);
    }
}