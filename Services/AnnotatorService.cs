using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Repositories;
using Task = SWP_BE.Models.Task;

namespace SWP_BE.Services
{
    public class AnnotatorService
    {
        private readonly IAnnotatorRepository _repo;
        public AnnotatorService(IAnnotatorRepository repo) { _repo = repo; }

        public async Task<IEnumerable<AnnotatorTaskDto>> GetTasks(Guid userId, string? status)
        {
            var tasks = await _repo.GetTasksAsync(userId, status);
            return tasks.Select(t => new AnnotatorTaskDto { TaskID = t.TaskID, TaskName = t.TaskName, Status = t.Status, Deadline = t.Deadline, CurrentRound = t.CurrentRound });
        }

        public async Task<TaskDetailDto?> GetTaskDetail(Guid taskId, Guid userId)
        {
            var t = await _repo.GetTaskByIdAsync(taskId, userId);
            if (t == null) return null;
            return new TaskDetailDto
            {
                TaskID = t.TaskID,
                TaskName = t.TaskName,
                Status = t.Status,
                Deadline = t.Deadline,
                TaskItems = t.TaskItems.Select(ti => new TaskItemDto
                {
                    ItemID = ti.ItemID,
                    FileName = ti.DataItem?.FileName ?? "",
                    FilePath = ti.DataItem?.FilePath ?? "",
                    IsFlagged = ti.IsFlagged,
                    AnnotationData = ti.TaskItemDetails.FirstOrDefault()?.AnnotationData,
                    Content = ti.TaskItemDetails.FirstOrDefault()?.Content
                }).ToList()
            };
        }

        public async Task<bool> SaveAnnotation(Guid itemId, SaveAnnotationDto dto)
        {
            var item = await _repo.GetItemByIdAsync(itemId);
            if (item == null) return false;
            var detail = item.TaskItemDetails.FirstOrDefault();
            if (detail == null) item.TaskItemDetails.Add(new TaskItemDetail { AnnotationData = dto.AnnotationData, Content = dto.Content });
            else { detail.AnnotationData = dto.AnnotationData; detail.Content = dto.Content; }
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> SubmitTask(Guid taskId, Guid userId, bool isResubmit)
        {
            var task = await _repo.GetTaskByIdAsync(taskId, userId);
            if (task == null) return (false, "Task không tồn tại.");

            if (isResubmit && task.CurrentRound >= 3) return (false, "Đã quá 3 lần nộp lại.");

            // Check xem tất cả item đã được xử lý chưa (Gán nhãn xong HOẶC bị Flagged)
            if (task.TaskItems.Any(ti => !ti.IsFlagged && !ti.TaskItemDetails.Any()))
                return (false, "Bạn chưa hoàn thành tất cả các file trong Task.");

            task.Status = "Pending Review";
            if (isResubmit) task.CurrentRound++;
            await _repo.SaveChangesAsync();
            return (true, "Nộp bài thành công.");
        }
    }
}