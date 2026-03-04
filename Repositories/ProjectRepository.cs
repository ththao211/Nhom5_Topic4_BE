using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using MyTask = SWP_BE.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace SWP_BE.Repositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllByManagerIdAsync(Guid managerId);
        Task<Project?> GetByIdAndManagerAsync(Guid projectId, Guid managerId);
        Task AddAsync(Project project);
        Task UpdateAsync(Project project);
        Task SaveChangesAsync();
        Task AddDataItemsAsync(IEnumerable<DataItem> dataItems);
        Task<List<DataItem>> GetUnassignedDataAsync(Guid projectId, int takeCount);
        Task AddTaskAsync(MyTask task); // Dùng MyTask ở đây
        Task AddTaskItemsAsync(IEnumerable<TaskItem> taskItems);
    }

    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;
        public ProjectRepository(AppDbContext context) { _context = context; }

        public async Task<IEnumerable<Project>> GetAllByManagerIdAsync(Guid managerId)
        {
            return await _context.Projects
                .Include(p => p.DataItems)
                .Where(p => p.ManagerID == managerId)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAndManagerAsync(Guid projectId, Guid managerId)
        {
            return await _context.Projects
                .Include(p => p.DataItems)
                .FirstOrDefaultAsync(p => p.ProjectID == projectId && p.ManagerID == managerId);
        }

        public async Task AddAsync(Project project) { await _context.Projects.AddAsync(project); }

        public async Task UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync() { await _context.SaveChangesAsync(); }

        public async Task AddDataItemsAsync(IEnumerable<DataItem> dataItems)
        {
            await _context.DataItems.AddRangeAsync(dataItems);
        }

        public async Task<List<DataItem>> GetUnassignedDataAsync(Guid projectId, int takeCount)
        {
            return await _context.DataItems
                .Where(d => d.ProjectID == projectId && d.IsAssigned == false)
                .Take(takeCount)
                .ToListAsync();
        }

        public async Task AddTaskAsync(MyTask task)
        {
            // Sửa lỗi: Đổi LabelingTasks thành Tasks cho khớp với AppDbContext
            await _context.Tasks.AddAsync(task);
        }

        public async Task AddTaskItemsAsync(IEnumerable<TaskItem> taskItems)
        {
            await _context.TaskItems.AddRangeAsync(taskItems);
        }
    }
}