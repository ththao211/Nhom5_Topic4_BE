namespace SWP_BE.DTOs.AdminDTO
{
    public class CreateUserDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Expertise { get; set; }
        public int Role { get; set; }
    }
}