using GridLink.DTOs;
using GridLink.Helpers;
using GridLink.Models;
using GridLink.Repositories.Interfaces;
using GridLink.Services.Interfaces;
using static GridLink.DTOs.AuthDTOs;

namespace GridLink.Services
{
    public class AuthService : IAuthService

    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;

        public AuthService(IUserRepository userRepository, JwtHelper jwtHelper)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
        }

        public async Task<ApiResponse<UserResponseDTO>> RegisterProsumerAsync(RegisterProsumerDto dto)
        {

            if (await _userRepository.ExistsByNicAsync(dto.NIC))
            {
                return ApiResponse<UserResponseDTO>.FailureResult(
                    "A user with this National Identity Card (NIC) is already registered.");
            }


            if (await _userRepository.ExistsByEmailAsync(dto.Email))
            {
                return ApiResponse<UserResponseDTO>.FailureResult(
                    "A user with this Email address is already registered.");
            }

            var user = new Users
            {
                NIC = dto.NIC.Trim().ToUpper(),
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLower(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Address = dto.Address.Trim(),
                HashPassword = PasswordHasher.HashPassword(dto.Password),
                Role = UserRole.Prosumer,
                Status = UserStatus.PendingActivation,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            var responseDto = MapToResponseDto(user);
            return ApiResponse<UserResponseDTO>.SuccessResult(
                responseDto,
                "Registration successful! Your account is pending activation by a Backoffice officer.");
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDTO dto)
        {

            var user = await _userRepository.GetByIdentifierAsync(dto.Identifier);
            if (user == null)
            {
                return ApiResponse<AuthResponseDto>.FailureResult("Invalid NIC/Email or password.");
            }


            if (!PasswordHasher.VerifyPassword(dto.Password, user.HashPassword))
            {
                return ApiResponse<AuthResponseDto>.FailureResult("Invalid NIC/Email or password.");
            }

            if (user.Status == UserStatus.PendingActivation)
            {
                return ApiResponse<AuthResponseDto>.FailureResult(
                    "Your account is currently pending activation by a Backoffice officer. Please wait until approved.");
            }

            if (user.Status == UserStatus.Deactivated)
            {
                return ApiResponse<AuthResponseDto>.FailureResult(
                    "Your account has been deactivated. Only a Backoffice officer can reactivate your account.");
            }

            var (token, expiration) = _jwtHelper.GenerateToken(user);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                Expiration = expiration,
                User = MapToResponseDto(user)
            };

            return ApiResponse<AuthResponseDto>.SuccessResult(authResponse, "Login successful.");
        }


        private static UserResponseDTO MapToResponseDto(Users user)
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
