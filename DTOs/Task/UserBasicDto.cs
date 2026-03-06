namespace SWP_BE.DTOs
{
    public class UserBasicDto
    {
        public Guid UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Expertise { get; set; } 
        public int Score { get; set; }
    }
}