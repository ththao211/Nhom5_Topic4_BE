namespace SWP_BE.DTOs.Login
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public UserInfoDTO User { get; set; } = new();
    }

    public class UserInfoDTO
    {
        public Guid UserId { get; set; } 
        public string RoleName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
