using System.ComponentModel.DataAnnotations;

namespace GridLink.DTOs
{
    public class UserRCreateDTO
    {
        [Required(ErrorMessage = "NIC Required")]
        [StringLength(12, MinimumLength = 10, ErrorMessage = "NIC should between 10 - 12 characters")]
        public string NIC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name Required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is Required")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is Required")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is Required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;
    }

    public class UserResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string NIC { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "Name Required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name should between 2 - 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is Required")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string? Role { get; set; }
    }

    public class UpdateProfileDTO
    {
        [Required(ErrorMessage = "Name Required")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Mobile Number is Required")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

    }
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;
    }

}
