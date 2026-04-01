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
        private readonly ReputationService _reputationService;

        public LabelingTaskService(ILabelingTaskRepository taskRepo, AppDbContext context, ReputationService reputationService)
        {
            _taskRepo = taskRepo;
            _context = context;
            _reputationService = reputationService;
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
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectID == projectId);
            if (project == null)
                return (false, "Project không tồn tại.", null);

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

            // XỬ LÝ CHO ANNOTATOR
            if (dto.AnnotatorID.HasValue && task.AnnotatorID != dto.AnnotatorID.Value)
            {
                var newAnnotatorId = dto.AnnotatorID.Value;

                // Kiểm tra giới hạn 3 task dựa trên điểm uy tín
                var checkResult = await _reputationService.CanManagerAssignTask(newAnnotatorId);
                if (!checkResult.CanAssign)
                {
                    return (false, checkResult.Message, null);
                }

                // Cập nhật số lượng cho người mới
                var newAnnotator = await _context.Users.FindAsync(newAnnotatorId);
                if (newAnnotator != null) newAnnotator.CurrentTaskCount += 1;

                // Trừ số lượng cho người cũ (nếu có)
                if (task.AnnotatorID.HasValue)
                {
                    var oldAnnotator = await _context.Users.FindAsync(task.AnnotatorID.Value);
                    if (oldAnnotator != null && oldAnnotator.CurrentTaskCount > 0)
                        oldAnnotator.CurrentTaskCount -= 1;
                }

                task.AnnotatorID = newAnnotatorId;
            }

            // XỬ LÝ CHO REVIEWER
            if (dto.ReviewerID.HasValue && task.ReviewerID != dto.ReviewerID.Value)
            {
                var newReviewerId = dto.ReviewerID.Value;

                // Tăng số lượng cho Reviewer mới
                var newReviewer = await _context.Users.FindAsync(newReviewerId);
                if (newReviewer != null) newReviewer.CurrentTaskCount += 1;

                // Trừ số lượng cho Reviewer cũ (nếu có)
                if (task.ReviewerID.HasValue)
                {
                    var oldReviewer = await _context.Users.FindAsync(task.ReviewerID.Value);
                    if (oldReviewer != null && oldReviewer.CurrentTaskCount > 0)
                        oldReviewer.CurrentTaskCount -= 1;
                }

                task.ReviewerID = newReviewerId;
            }

            await _taskRepo.UpdateTaskAsync(task);
            await _taskRepo.SaveChangesAsync();

            // Ép lưu thay đổi của bảng Users (CurrentTaskCount)
            await _context.SaveChangesAsync();

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