using System.ComponentModel.DataAnnotations;
using static SWP_BE.Models.User;

namespace SWP_BE.DTOs
{
    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class ResetPasswordRequests
    {
        public string Email { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
    }

    public static class PasswordResetStore
    {
        public static Dictionary<string, Guid> ResetTokens = new();
    }


    // DTO hứng dữ liệu khi Admin tạo tài khoản mới
    public class UserCreateDTO
    {
        [Required] public string UserName { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public UserRole Role { get; set; } = 0; 
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        public string? Expertise { get; set; }
    }

    // DTO hứng dữ liệu khi Admin sửa tài khoản (không bắt buộc đổi Pass)
    public class UserUpdateDTO
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public UserRole Role { get; set; } = 0;
        public string? Expertise { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO trả dữ liệu về cho Frontend (Tuyệt đối không có Password)
    public class UserResponseDTO
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = 0;
        public string Email { get; set; } = string.Empty;
        public string? Expertise { get; set; }
        public int Score { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserScoreResponseDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = 0;

        public int CurrentScore { get; set; }
        public int TotalScoreChange { get; set; }

        public int TaskCount { get; set; }
    }

    public class UserProfileDto
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; } = 0;
        public int Score { get; set; }
        public int CurrentTaskCount { get; set; }
        public bool IsActive { get; set; }
    }
}