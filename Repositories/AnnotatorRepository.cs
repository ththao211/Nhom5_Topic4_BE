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

        void DeleteItemDetails(IEnumerable<TaskItemDetail> details);

        System.Threading.Tasks.Task AddDisputeAsync(Dispute dispute);
        System.Threading.Tasks.Task<User?> GetUserWithLogsAsync(Guid userId);
        System.Threading.Tasks.Task SaveChangesAsync();

        // 🔥 Đổi kiểu trả về thành object để chứa thêm EvidenceImages
        System.Threading.Tasks.Task<IEnumerable<object>> GetDisputesByUserIdAsync(Guid userId);
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

        // ==========================================================
        // 🔥 HÀM NÀY ĐÃ ĐƯỢC FIX ĐỂ LẤY KÈM ẢNH BẰNG CHỨNG
        // ==========================================================
        public async System.Threading.Tasks.Task<IEnumerable<object>> GetDisputesByUserIdAsync(Guid userId)
        {
            var rawData = await _context.Disputes
                .Where(d => d.UserID == userId)
                .OrderByDescending(d => d.CreatedAt) // Mới nhất xếp lên đầu
                .Select(d => new
                {
                    DisputeID = d.DisputeID,
                    TaskID = d.TaskID,
                    TaskName = d.Task != null ? d.Task.TaskName : "Unknown",
                    ProjectName = (d.Task != null && d.Task.Project != null) ? d.Task.Project.ProjectName : "Unknown",
                    Reason = d.Reason,
                    ManagerComment = d.ManagerComment,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    ResolvedAt = d.ResolvedAt,

                    EvidenceString = _context.ReviewComments
                        .Where(rc => rc.ReviewHistory.TaskID == d.TaskID && rc.ReviewHistory.FinalResult == "DisputeEvidence")
                        .OrderByDescending(rc => rc.CreatedAt)
                        .Select(rc => rc.EvidenceImages)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Ép string ảnh thành List<string> để FE parse dễ dàng
            return rawData.Select(d => new
            {
                DisputeID = d.DisputeID,
                TaskID = d.TaskID,
                TaskName = d.TaskName,
                ProjectName = d.ProjectName,
                Reason = d.Reason,
                ManagerComment = d.ManagerComment,
                Status = d.Status,
                CreatedAt = d.CreatedAt,
                ResolvedAt = d.ResolvedAt,
                EvidenceImages = string.IsNullOrEmpty(d.EvidenceString)
                    ? new List<string>()
                    : new List<string> { d.EvidenceString }
            });
        }

        public async System.Threading.Tasks.Task<TaskItem?> GetItemByIdAsync(Guid itemId)
        {
            return await _context.TaskItems
                .Include(ti => ti.DataItem)
                .Include(ti => ti.TaskItemDetails)
                .Include(ti => ti.Task)
                .FirstOrDefaultAsync(ti => ti.ItemID == itemId);
        }

        public void DeleteItemDetails(IEnumerable<TaskItemDetail> details)
        {
            _context.TaskItemDetails.RemoveRange(details);
        }

        public async System.Threading.Tasks.Task AddDisputeAsync(Dispute dispute) => await _context.Disputes.AddAsync(dispute);
        public async System.Threading.Tasks.Task<User?> GetUserWithLogsAsync(Guid userId) => await _context.Users.Include(u => u.ReputationLogs).FirstOrDefaultAsync(u => u.UserID == userId);
        public async System.Threading.Tasks.Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}