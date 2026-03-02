using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Repositories;

namespace SWP_BE.Services
{
    public interface ILabelingTaskService
    {
        Task<IEnumerable<UnassignedDataItemDto>> GetUnassignedDataAsync(Guid projectId);
        Task<(bool success, string message, Guid? taskId)> CreateTaskAsync(Guid projectId, CreateTaskDto dto);
        Task<(bool success, string message)> AssignPersonnelAsync(Guid taskId, AssignTaskDto dto);
        Task<IEnumerable<TaskProgressDto>> GetProjectTasksAsync(Guid projectId);
        Task<(bool success, string message)> UpdateDeadlineAsync(Guid taskId, UpdateDeadlineDto dto);
    }

    public class LabelingTaskService : ILabelingTaskService
    {
        private readonly ILabelingTaskRepository _taskRepo;

        public LabelingTaskService(ILabelingTaskRepository taskRepo)
        {
            _taskRepo = taskRepo;
        }

        // API 1
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

        // API 2
        public async Task<(bool success, string message, Guid? taskId)> CreateTaskAsync(Guid projectId, CreateTaskDto dto)
        {
            var dataItems = await _taskRepo.GetDataItemsByIdsAsync(projectId, dto.DataIDs);

            // Check nếu có file nào đã bị gán trước đó hoặc không tồn tại
            if (dataItems.Count != dto.DataIDs.Count || dataItems.Any(d => d.IsAssigned))
            {
                return (false, "Một số dữ liệu không tồn tại hoặc đã được phân công.", null);
            }

            // 1. Tạo bảng LabelingTask
            var newTask = new Models.Tasks
            {
                TaskID = Guid.NewGuid(),
                ProjectID = projectId,
                TaskName = dto.TaskName,
                Status = "NEW", // Theo chuẩn thiết kế của bạn
                Deadline = dto.Deadline ?? DateTime.UtcNow.AddDays(7), // Mặc định 7 ngày nếu không truyền
                RateComplete = 0,
                RejectCount = 0
            };

            // 2. Gom vào TaskItems & 3. Đánh dấu DataItems IsAssigned = true
            var taskItems = new List<TaskItem>();
            foreach (var item in dataItems)
            {
                taskItems.Add(new TaskItem
                {
                    ItemID = Guid.NewGuid(),
                    TaskID = newTask.TaskID,
                    DataID = item.DataID,
                    IsFlagged = false
                });

                item.IsAssigned = true; // Đánh dấu đã có chủ
            }

            await _taskRepo.CreateTaskWithItemsAsync(newTask, taskItems, dataItems);

            return (true, "Tạo task thành công.", newTask.TaskID);
        }

        // API 3
        public async Task<(bool success, string message)> AssignPersonnelAsync(Guid taskId, AssignTaskDto dto)
        {
            var task = await _taskRepo.GetTaskByIdAsync(taskId);
            if (task == null) return (false, "Task không tồn tại.");

            if (dto.AnnotatorID.HasValue) task.AnnotatorID = dto.AnnotatorID.Value;
            if (dto.ReviewerID.HasValue) task.ReviewerID = dto.ReviewerID.Value;

            await _taskRepo.UpdateTaskAsync(task);
            await _taskRepo.SaveChangesAsync();

            return (true, "Phân công nhân sự thành công.");
        }

        // API 4
        public async Task<IEnumerable<TaskProgressDto>> GetProjectTasksAsync(Guid projectId)
        {
            var tasks = await _taskRepo.GetTasksByProjectIdAsync(projectId);
            return tasks.Select(t => new TaskProgressDto
            {
                TaskID = t.TaskID,
                TaskName = t.TaskName,
                Status = t.Status,
                RateComplete = t.RateComplete,
                RejectCount = t.RejectCount,
                Deadline = t.Deadline,
                AnnotatorID = t.AnnotatorID,
                ReviewerID = t.ReviewerID,
                TotalItems = t.TaskItems?.Count ?? 0
            });
        }

        // API 5
        public async Task<(bool success, string message)> UpdateDeadlineAsync(Guid taskId, UpdateDeadlineDto dto)
        {
            var task = await _taskRepo.GetTaskByIdAsync(taskId);
            if (task == null) return (false, "Task không tồn tại.");

            task.Deadline = dto.Deadline;

            await _taskRepo.UpdateTaskAsync(task);
            await _taskRepo.SaveChangesAsync();

            return (true, "Cập nhật thời hạn thành công.");
        }
    }
}