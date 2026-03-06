using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using System.Collections.Generic;
using System.Linq;
// Giải quyết xung đột: Ép C# hiểu Task là của hệ thống, 
// còn MyTask là cái bảng Task của bạn trong Database.
using Task = System.Threading.Tasks.Task;
using MyTask = SWP_BE.Models.Task;

namespace SWP_BE.Repositories
{
    public interface ILabelingTaskRepository
    {
        Task<IEnumerable<DataItem>> GetUnassignedDataByProjectIdAsync(Guid projectId);
        Task<List<DataItem>> GetDataItemsByIdsAsync(Guid projectId, List<Guid> dataIds);
        Task<MyTask?> GetTaskByIdAsync(Guid taskId);
        Task<IEnumerable<MyTask>> GetTasksByProjectIdAsync(Guid projectId);
        Task CreateTaskWithItemsAsync(MyTask task, List<TaskItem> taskItems, List<DataItem> updatedDataItems);
        Task UpdateTaskAsync(MyTask task);
        Task SaveChangesAsync();
    }

    public class LabelingTaskRepository : ILabelingTaskRepository
    {
        private readonly AppDbContext _context;
        public LabelingTaskRepository(AppDbContext context) { _context = context; }

        public async Task<IEnumerable<DataItem>> GetUnassignedDataByProjectIdAsync(Guid projectId)
        {
            return await _context.DataItems
                .Where(d => d.ProjectID == projectId && d.IsAssigned == false)
                .ToListAsync();
        }

        public async Task<List<DataItem>> GetDataItemsByIdsAsync(Guid projectId, List<Guid> dataIds)
        {
            return await _context.DataItems
                .Where(d => d.ProjectID == projectId && dataIds.Contains(d.DataID))
                .ToListAsync();
        }

        public async Task<MyTask?> GetTaskByIdAsync(Guid taskId)
        {
            // Sửa lỗi: Đổi LabelingTasks thành Tasks cho khớp với AppDbContext mới
            return await _context.Tasks.FindAsync(taskId);
        }

        public async Task<IEnumerable<MyTask>> GetTasksByProjectIdAsync(Guid projectId)
        {
            // Sửa lỗi: Đổi LabelingTasks thành Tasks
            return await _context.Tasks
                .Include(t => t.TaskItems)
                .Where(t => t.ProjectID == projectId)
                .ToListAsync();
        }

        public async Task CreateTaskWithItemsAsync(MyTask task, List<TaskItem> taskItems, List<DataItem> updatedDataItems)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Sửa lỗi: Đổi LabelingTasks thành Tasks
                await _context.Tasks.AddAsync(task);
                await _context.TaskItems.AddRangeAsync(taskItems);
                _context.DataItems.UpdateRange(updatedDataItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateTaskAsync(MyTask task)
        {
            // Sửa lỗi: Đổi LabelingTasks thành Tasks
            _context.Tasks.Update(task);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync() { await _context.SaveChangesAsync(); }
    }
}