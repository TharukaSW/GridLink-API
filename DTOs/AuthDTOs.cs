using System.ComponentModel.DataAnnotations;

namespace GridLink.DTOs
{
    public class AuthDTOs
    {
        public class RegisterProsumerDto
        {
            [Required(ErrorMessage = "NIC Required")]
            [StringLength(12, MinimumLength = 10, ErrorMessage = "NIC should between 10 - 12 characters")]
            public string NIC { get; set; } = string.Empty;

            [Required(ErrorMessage = "Name Required")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Name should between 2 - 100 characters")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email Address Required")]
            [EmailAddress (ErrorMessage = "Invalid Email Format")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mobile Number is Required")]
            [Phone(ErrorMessage = "Invalid Mobile Number Format")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Address Required")]
            public string Address { get; set; } = string.Empty;

            [Required(ErrorMessage ="Password is Required")]
            [MinLength(8, ErrorMessage ="Password must be at least 8 characters")]
            public string Password { get; set; } = string.Empty;
        }

        public class LoginDTO
        {
            [Required(ErrorMessage = "NIC Required")]
            public string Identifier { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is Required")]
            public string Password { get; set; } = string.Empty;
        }

        public class AuthResponseDto
        {
            public string Token { get; set; } = string.Empty;
            public DateTime Expiration { get; set; }
            public UserResponseDTO User { get; set; } = null!;
        }
    }
}
