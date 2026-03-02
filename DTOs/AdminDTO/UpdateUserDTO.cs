namespace SWP_BE.DTOs.AdminDTO
{
    public class UpdateUserDTO
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Expertise { get; set; }
        public int? Score { get; set; }
        public int? CurrentTaskCount { get; set; }
        public bool? IsActive { get; set; }
        public int Role { get; set; }
    }
}