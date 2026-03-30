using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using TaskModel = SWP_BE.Models.Task;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SWP_BE.Services
{
    public interface IProgressService
    {
        System.Threading.Tasks.Task UpdateTaskProgress(Guid taskId);
        System.Threading.Tasks.Task UpdateProjectProgress(Guid projectId);
        System.Threading.Tasks.Task UpdateTaskAndProject(Guid taskId);
    }

    public class ProgressService : IProgressService
    {
        private readonly AppDbContext _context;

        public ProgressService(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // Update Task Progress
        // =====================================================
        public async System.Threading.Tasks.Task UpdateTaskProgress(Guid taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskItems)
                    .ThenInclude(i => i.TaskItemDetails)
                .FirstOrDefaultAsync(t => t.TaskID == taskId);

            if (task == null) return;

            var totalItems = task.TaskItems.Count;

            if (totalItems == 0) return;

            // Annotator đã submit
            var annotatedItems = task.TaskItems
                .Count(i =>
                    !i.IsFlagged &&
                    i.TaskItemDetails.Any());

            // Reviewer đã approve
            var approvedItems = task.TaskItems
                .Count(i =>
                 i.TaskItemDetails.Any() &&
                 i.TaskItemDetails.All(d => d.IsApproved == "True"));
            // ==============================
            // Final Progress (Reviewer)
            // ==============================

            task.RateComplete = (double)approvedItems / totalItems * 100;

            await _context.SaveChangesAsync();
        }

        // =====================================================
        // Update Project Progress
        // =====================================================
        public async System.Threading.Tasks.Task UpdateProjectProgress(Guid projectId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.TaskItems)
                .Where(t => t.ProjectID == projectId)
                .ToListAsync();

            if (!tasks.Any()) return;

            // Đã xóa dòng: var projectProgress = tasks.Average(t => t.RateComplete); bị lỗi trùng lặp ở đây

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == projectId);

            if (project == null) return;

            var totalItems = tasks.Sum(t => t.TaskItems?.Count ?? 0);
            double projectProgress = 0;

            if (totalItems > 0)
            {
                double completedItems = tasks.Sum(t => (t.TaskItems?.Count ?? 0) * (t.RateComplete / 100.0));
                projectProgress = (completedItems / totalItems) * 100;
            }

            // CHỈ GÁN 3 TRẠNG THÁI GỐC (Thay "ProjectStatus" bằng tên Enum của bạn nếu cần)
            if (projectProgress >= 100)
            {
                project.Status = "Closed";
            }
            else if (projectProgress > 0 && project.Status == "Open")
            {
                project.Status = "Processing";
            }

            await _context.SaveChangesAsync();
        }

        // =====================================================
        // Update Task + Project
        // =====================================================
        public async System.Threading.Tasks.Task UpdateTaskAndProject(Guid taskId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskID == taskId);

            if (task == null) return;

            await UpdateTaskProgress(taskId);

            await UpdateProjectProgress(task.ProjectID);
        }
    }
}