using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SWP_BE.Repositories
{
    public interface IAnnotatorRepository
    {
        System.Threading.Tasks.Task<IEnumerable<SWP_BE.Models.Task>> GetTasksAsync(Guid annotatorId, string? status);
        System.Threading.Tasks.Task<SWP_BE.Models.Task?> GetTaskByIdAsync(Guid taskId, Guid annotatorId);
        System.Threading.Tasks.Task<TaskItem?> GetItemByIdAsync(Guid itemId);

        // THÊM MỚI: Phương thức để xóa các AnnotationDetail cũ
        void DeleteItemDetails(IEnumerable<TaskItemDetail> details);

        System.Threading.Tasks.Task AddDisputeAsync(Dispute dispute);
        System.Threading.Tasks.Task<User?> GetUserWithLogsAsync(Guid userId);
        System.Threading.Tasks.Task SaveChangesAsync();
    }

    public class AnnotatorRepository : IAnnotatorRepository
    {
        private readonly AppDbContext _context;
        public AnnotatorRepository(AppDbContext context) { _context = context; }

        public async System.Threading.Tasks.Task<IEnumerable<SWP_BE.Models.Task>> GetTasksAsync(Guid annotatorId, string? status)
        {
            var query = _context.Tasks.Where(t => t.AnnotatorID == annotatorId);
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<SWP_BE.Models.Task.TaskStatus>(status, true, out var parsedStatus))
                {
                    query = query.Where(t => t.Status == parsedStatus);
                }
            }
            return await query.ToListAsync();
        }

        public async System.Threading.Tasks.Task<SWP_BE.Models.Task?> GetTaskByIdAsync(Guid taskId, Guid annotatorId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectLabels)
                        .ThenInclude(pl => pl.Label)
                .Include(t => t.TaskItems)
                    .ThenInclude(ti => ti.DataItem)
                .Include(t => t.TaskItems)
                    .ThenInclude(ti => ti.TaskItemDetails)
                .FirstOrDefaultAsync(t => t.TaskID == taskId && t.AnnotatorID == annotatorId);
        }

        public async System.Threading.Tasks.Task<TaskItem?> GetItemByIdAsync(Guid itemId)
        {
            return await _context.TaskItems
                .Include(ti => ti.DataItem)
                .Include(ti => ti.TaskItemDetails)
                .FirstOrDefaultAsync(ti => ti.ItemID == itemId);
        }

        // THỰC THI XÓA: Dùng RemoveRange để xóa hẳn khỏi DB
        public void DeleteItemDetails(IEnumerable<TaskItemDetail> details)
        {
            _context.TaskItemDetails.RemoveRange(details);
        }

        public async System.Threading.Tasks.Task AddDisputeAsync(Dispute dispute) => await _context.Disputes.AddAsync(dispute);
        public async System.Threading.Tasks.Task<User?> GetUserWithLogsAsync(Guid userId) => await _context.Users.Include(u => u.ReputationLogs).FirstOrDefaultAsync(u => u.UserID == userId);
        public async System.Threading.Tasks.Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}