using System.ComponentModel.DataAnnotations;

namespace SWP_BE.DTOs.AdminDTO
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Username là bắt buộc")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Username phải từ 5-50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password là bắt buộc")]
        [MinLength(6, ErrorMessage = "Password phải tối thiểu 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "FullName là bắt buộc")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Expertise { get; set; }

        [Required(ErrorMessage = "Role là bắt buộc")]
        [Range(1, 4, ErrorMessage = "Role phải từ 1-4 (1:Admin, 2:Manager, 3:Annotator, 4:Reviewer)")]
        public int Role { get; set; }
    }
}