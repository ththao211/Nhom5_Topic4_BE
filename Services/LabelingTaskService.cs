using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Repositories;
using TaskModel = SWP_BE.Models.Task;

namespace SWP_BE.Services
{
    public interface ILabelingTaskService
    {
        Task<IEnumerable<UnassignedDataItemDto>> GetUnassignedDataAsync(Guid projectId);
        Task<(bool success, string message, Guid? taskId)> CreateTaskAsync(Guid projectId, CreateTaskDto dto);
        Task<(bool success, string message, TaskModel? taskDetails)> AssignPersonnelAsync(Guid taskId, AssignTaskDto dto);
        Task<IEnumerable<TaskProgressDto>> GetProjectTasksAsync(Guid projectId);
        Task<(bool success, string message)> UpdateDeadlineAsync(Guid taskId, UpdateDeadlineDto dto);
        Task<IEnumerable<UserBasicDto>> GetUsersByRoleAsync(string roleName);
    }

    public class LabelingTaskService : ILabelingTaskService
    {
        private readonly ILabelingTaskRepository _taskRepo;
        private readonly AppDbContext _context;

        public LabelingTaskService(ILabelingTaskRepository taskRepo, AppDbContext context)
        {
            _taskRepo = taskRepo;
            _context = context;
        }

        public async Task<IEnumerable<UnassignedDataItemDto>> GetUnassignedDataAsync(Guid projectId)
        {
            var dataItems = await _taskRepo.GetUnassignedDataByProjectIdAsync(projectId);
            return dataItems.Select(d => new UnassignedDataItemDto
            {
                DataID = d.DataID,
                FileName = d.FileName,
                FilePath = d.FilePath,
                FileType = d.FileType
            });
        }

        public async Task<(bool success, string message, Guid? taskId)> CreateTaskAsync(Guid projectId, CreateTaskDto dto)
        {
            var dataItems = await _taskRepo.GetDataItemsByIdsAsync(projectId, dto.DataIDs);
            if (dataItems.Count != dto.DataIDs.Count || dataItems.Any(d => d.IsAssigned))
            {
                return (false, "Một số dữ liệu không tồn tại hoặc đã được phân công.", null);
            }

            var newTask = new TaskModel
            {
                TaskID = Guid.NewGuid(),
                ProjectID = projectId,
                TaskName = dto.TaskName,
                Status = TaskModel.TaskStatus.New,
                Deadline = dto.Deadline ?? DateTime.UtcNow.AddDays(7),
                RateComplete = 0
            };

            var taskItems = dataItems.Select(item => new TaskItem
            {
                ItemID = Guid.NewGuid(),
                TaskID = newTask.TaskID,
                DataID = item.DataID,
                IsFlagged = false
            }).ToList();

            foreach (var item in dataItems) item.IsAssigned = true;

            await _taskRepo.CreateTaskWithItemsAsync(newTask, taskItems, dataItems);
            return (true, "Tạo task thành công.", newTask.TaskID);
        }

        public async Task<(bool success, string message, TaskModel? taskDetails)> AssignPersonnelAsync(Guid taskId, AssignTaskDto dto)
        {
            var task = await _taskRepo.GetTaskByIdAsync(taskId);
            if (task == null) return (false, "Task không tồn tại.", null);

            if (dto.AnnotatorID.HasValue) task.AnnotatorID = dto.AnnotatorID.Value;
            if (dto.ReviewerID.HasValue) task.ReviewerID = dto.ReviewerID.Value;

            await _taskRepo.UpdateTaskAsync(task);
            await _taskRepo.SaveChangesAsync();

            var fullTaskInfo = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Annotator)
                .Include(t => t.Reviewer)
                .FirstOrDefaultAsync(t => t.TaskID == taskId);

            return (true, "Phân công nhân sự thành công.", fullTaskInfo);
        }

        public async Task<IEnumerable<TaskProgressDto>> GetProjectTasksAsync(Guid projectId)
        {
            var tasks = await _taskRepo.GetTasksByProjectIdAsync(projectId);
            return tasks.Select(t => new TaskProgressDto
            {
                TaskID = t.TaskID,
                TaskName = t.TaskName,
                Status = t.Status.ToString(),
                RateComplete = t.RateComplete,
                Deadline = t.Deadline,
                AnnotatorID = t.AnnotatorID,
                ReviewerID = t.ReviewerID,
                TotalItems = t.TaskItems?.Count ?? 0,
                AnnotatorName = t.Annotator?.FullName,
                ReviewerName = t.Reviewer?.FullName
            });
        }

        public async Task<(bool success, string message)> UpdateDeadlineAsync(Guid taskId, UpdateDeadlineDto dto)
        {
            var task = await _taskRepo.GetTaskByIdAsync(taskId);
            if (task == null) return (false, "Task không tồn tại.");
            task.Deadline = dto.Deadline;
            await _taskRepo.UpdateTaskAsync(task);
            await _taskRepo.SaveChangesAsync();
            return (true, "Cập nhật thời hạn thành công.");
        }

        // HÀM LẤY USER THEO ROLE
        public async Task<IEnumerable<UserBasicDto>> GetUsersByRoleAsync(string roleName)
        {
            if (!Enum.TryParse<Models.User.UserRole>(roleName, out var roleEnum))
            {
                return new List<UserBasicDto>();
            }

            return await _context.Users
                .Where(u => u.Role == roleEnum && u.IsActive == true)
                .Select(u => new UserBasicDto
                {
                    UserID = u.UserID,
                    FullName = u.FullName,
                    Email = u.Email,
                    Expertise = u.Expertise,
                    Score = u.Score
                })
                .ToListAsync();
        }
    }
}