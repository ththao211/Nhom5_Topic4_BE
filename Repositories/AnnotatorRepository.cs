using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using Task = SWP_BE.Models.Task; // Tránh nhầm lẫn với System.Threading.Tasks.Task

namespace SWP_BE.Repositories
{
    public interface IAnnotatorRepository
    {
        Task<IEnumerable<Task>> GetTasksAsync(Guid annotatorId, string? status);
        Task<Task?> GetTaskByIdAsync(Guid taskId, Guid annotatorId);
        Task<TaskItem?> GetItemByIdAsync(Guid itemId);
        System.Threading.Tasks.Task AddDisputeAsync(Dispute dispute);
        Task<User?> GetUserWithLogsAsync(Guid userId);
        System.Threading.Tasks.Task SaveChangesAsync();
    }

    public class AnnotatorRepository : IAnnotatorRepository
    {
        private readonly AppDbContext _context;
        public AnnotatorRepository(AppDbContext context) { _context = context; }

        public async Task<IEnumerable<Task>> GetTasksAsync(Guid annotatorId, string? status)
        {
            var query = _context.Tasks.Where(t => t.AnnotatorID == annotatorId);
            if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
            return await query.ToListAsync();
        }

        public async Task<Task?> GetTaskByIdAsync(Guid taskId, Guid annotatorId)
        {
            return await _context.Tasks
                .Include(t => t.TaskItems)
                    .ThenInclude(ti => ti.DataItem)
                .Include(t => t.TaskItems)
                    .ThenInclude(ti => ti.TaskItemDetails) // Cần dòng Collection mình nhắc ở TaskItem
                .FirstOrDefaultAsync(t => t.TaskID == taskId && t.AnnotatorID == annotatorId);
        }

        public async Task<TaskItem?> GetItemByIdAsync(Guid itemId)
        {
            return await _context.TaskItems
                .Include(ti => ti.DataItem)
                .Include(ti => ti.TaskItemDetails)
                .FirstOrDefaultAsync(ti => ti.ItemID == itemId);
        }

        public async System.Threading.Tasks.Task AddDisputeAsync(Dispute dispute) => await _context.Disputes.AddAsync(dispute);

        public async Task<User?> GetUserWithLogsAsync(Guid userId)
        {
            return await _context.Users.Include(u => u.ReputationLogs).FirstOrDefaultAsync(u => u.UserID == userId);
        }

        public async System.Threading.Tasks.Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}