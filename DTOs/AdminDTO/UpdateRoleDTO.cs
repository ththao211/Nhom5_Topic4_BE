namespace SWP_BE.DTOs.AdminDTO
{
    public class UpdateRoleDTO
    {
        public int Role { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}