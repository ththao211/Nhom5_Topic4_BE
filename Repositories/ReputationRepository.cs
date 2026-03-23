using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;

namespace SWP_BE.Repositories
{
    public interface IReputationRepository
    {
        Task<User?> GetUserForUpdateAsync(Guid userId);
        Task<List<ReputationRule>> GetAllActiveRulesAsync();
        Task<List<ReputationLog>> GetLatestFailLogsAsync(Guid userId, int count);


        System.Threading.Tasks.Task AddLogAsync(ReputationLog log);
        System.Threading.Tasks.Task SaveChangesAsync();
        // Trong IReputationRepository.cs
        Task<List<ReputationRule>> GetAllRulesAsync(); // Lấy tất cả kể cả Inactive
        Task<ReputationRule?> GetRuleByIdAsync(int ruleId);
    }

    public class ReputationRepository : IReputationRepository
    {
        private readonly AppDbContext _context;
        public ReputationRepository(AppDbContext context) => _context = context;

        public async Task<User?> GetUserForUpdateAsync(Guid userId)
            => await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);

        public async Task<List<ReputationRule>> GetAllActiveRulesAsync()
            => await _context.ReputationRules.Where(r => r.IsActive).ToListAsync();

        public async Task<List<ReputationLog>> GetLatestFailLogsAsync(Guid userId, int count)
        {
            return await _context.ReputationLogs
                .Where(l => l.UserID == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task AddLogAsync(ReputationLog log) => await _context.ReputationLogs.AddAsync(log);

        public async System.Threading.Tasks.Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<List<ReputationRule>> GetAllRulesAsync()
        {
            return await _context.ReputationRules.ToListAsync();
        }

        public async Task<ReputationRule?> GetRuleByIdAsync(int ruleId)
        {
            return await _context.ReputationRules.FindAsync(ruleId);
        }
    }
}