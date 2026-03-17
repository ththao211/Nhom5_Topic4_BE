using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using static SWP_BE.Models.User;

namespace SWP_BE.Repositories
{
    public class SuggestionRepository
    {
        private readonly AppDbContext _context;
        public SuggestionRepository(AppDbContext context) => _context = context;

        public async Task<Project?> GetProjectAsync(Guid projectId)
            => await _context.Projects.FindAsync(projectId);

        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            // 1. Chuyển đổi string 'role' sang kiểu Enum UserRole
            if (!Enum.TryParse<UserRole>(role, true, out var roleEnum))
            {
                return new List<User>(); // Trả về list rỗng nếu string không hợp lệ
            }
            return await _context.Users
                .Include(u => u.AnnotatorStat)
                .Include(u => u.ReviewerStat)
                .Where(u => u.Role == roleEnum && u.IsActive)
                .ToListAsync();
        }
    }
}