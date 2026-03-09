using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Repositories;

namespace SWP_BE.Services
{
    public class AnnotatorService
    {
        private readonly IAnnotatorRepository _repo;
        public AnnotatorService(IAnnotatorRepository repo) { _repo = repo; }

        public async System.Threading.Tasks.Task<IEnumerable<AnnotatorTaskDto>> GetTasks(Guid userId, string? status)
        {
            var tasks = await _repo.GetTasksAsync(userId, status);
            return tasks.Select(t => new AnnotatorTaskDto
            {
                TaskID = t.TaskID,
                TaskName = t.TaskName,
                Status = t.Status.ToString(),
                Deadline = t.Deadline,
                CurrentRound = t.CurrentRound
            });
        }

        public async System.Threading.Tasks.Task<TaskDetailDto?> GetTaskDetail(Guid taskId, Guid userId)
        {
            var t = await _repo.GetTaskByIdAsync(taskId, userId);
            if (t == null) return null;

            return new TaskDetailDto
            {
                TaskID = t.TaskID,
                TaskName = t.TaskName,
                Status = t.Status.ToString(),
                Deadline = t.Deadline,
                CurrentRound = t.CurrentRound,
                TaskItems = t.TaskItems.Select(ti => new TaskItemDto
                {
                    ItemID = ti.ItemID,
                    FileName = ti.DataItem?.FileName ?? "Unknown File",
                    FilePath = ti.DataItem?.FilePath ?? "",
                    IsFlagged = ti.IsFlagged,
                    Annotations = (ti.TaskItemDetails ?? new List<TaskItemDetail>()).Select(d => new AnnotationDetailDto
                    {
                        AnnotationData = d.AnnotationData,
                        Content = d.Content,
                        Field = d.Field
                    }).ToList()
                }).ToList(),

                AvailableLabels = t.Project?.ProjectLabels?
                    .Where(pl => pl.Label != null && !string.IsNullOrEmpty(pl.Label.LabelName))
                    .Select(pl => new LabelInfoDto
                    {
                        Name = !string.IsNullOrEmpty(pl.CustomName) ? pl.CustomName : pl.Label.LabelName,
                        Color = !string.IsNullOrEmpty(pl.Label.DefaultColor) ? pl.Label.DefaultColor : "#ffffff"
                    })
                    .ToList() ?? new List<LabelInfoDto>()
            };
        }

        public async System.Threading.Tasks.Task<bool> SaveAnnotation(Guid itemId, SaveAnnotationDto dto)
        {
            var item = await _repo.GetItemByIdAsync(itemId);
            if (item == null) return false;

            try
            {
                // 1. Xóa sạch các bản ghi cũ khỏi Database thông qua Repository
                if (item.TaskItemDetails != null && item.TaskItemDetails.Any())
                {
                    _repo.DeleteItemDetails(item.TaskItemDetails);
                }

                // 2. Thêm mới danh sách tọa độ từ Frontend
                if (dto.Annotations != null)
                {
                    foreach (var ann in dto.Annotations)
                    {
                        item.TaskItemDetails.Add(new TaskItemDetail
                        {
                            AnnotationData = ann.AnnotationData,
                            Content = ann.Content,
                            Field = ann.Field,
                            TaskItemID = itemId
                        });
                    }
                }

                // Thực thi lưu
                await _repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Ném lỗi để log Server ghi lại nguyên nhân thực sự (thường là tràn NVARCHAR)
                throw new Exception($"Lỗi lưu Database: {ex.Message}", ex);
            }
        }

        public async System.Threading.Tasks.Task<(bool Success, string Message)> SubmitTask(Guid taskId, Guid userId, bool isResubmit)
        {
            var task = await _repo.GetTaskByIdAsync(taskId, userId);
            if (task == null) return (false, "Task không tồn tại.");
            if (isResubmit && task.CurrentRound >= 3) return (false, "Đã quá 3 lần nộp lại.");

            var items = task.TaskItems ?? new List<TaskItem>();
            if (items.Any(ti => !ti.IsFlagged && !(ti.TaskItemDetails?.Any() ?? false)))
                return (false, "Bạn chưa hoàn thành tất cả các file trong Task.");

            task.Status = SWP_BE.Models.Task.TaskStatus.PendingReview;
            if (isResubmit) task.CurrentRound++;

            await _repo.SaveChangesAsync();
            return (true, "Nộp bài thành công.");
        }

        public async System.Threading.Tasks.Task<bool> StartTask(Guid taskId, Guid userId)
        {
            var task = await _repo.GetTaskByIdAsync(taskId, userId);
            if (task == null) return false;
            if (task.Status == SWP_BE.Models.Task.TaskStatus.New)
            {
                task.Status = SWP_BE.Models.Task.TaskStatus.InProgress;
                await _repo.SaveChangesAsync();
            }
            return true;
        }

        public async System.Threading.Tasks.Task<bool> FlagItem(Guid itemId)
        {
            var item = await _repo.GetItemByIdAsync(itemId);
            if (item == null) return false;
            item.IsFlagged = true;
            await _repo.SaveChangesAsync();
            return true;
        }

        public async System.Threading.Tasks.Task<bool> CreateDispute(Guid taskId, Guid userId, DisputeRequestDto dto)
        {
            var task = await _repo.GetTaskByIdAsync(taskId, userId);
            if (task == null) return false;

            var dispute = new Dispute
            {
                DisputeID = Guid.NewGuid(),
                TaskID = taskId,
                UserID = userId,
                Reason = dto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddDisputeAsync(dispute);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async System.Threading.Tasks.Task<ReputationResponseDto?> GetReputation(Guid userId)
        {
            var user = await _repo.GetUserWithLogsAsync(userId);
            if (user == null) return null;

            return new ReputationResponseDto
            {
                CurrentScore = user.Score,
                Logs = (user.ReputationLogs ?? new List<ReputationLog>())
                    .OrderByDescending(l => l.CreatedAt)
                    .Select(l => new ReputationLogDto
                    {
                        ScoreChange = l.ScoreChange,
                        Reason = l.Reason,
                        CreatedAt = l.CreatedAt
                    }).ToList()
            };
        }

        public async System.Threading.Tasks.Task<TaskItemDto?> GetItemDetail(Guid itemId)
        {
            var ti = await _repo.GetItemByIdAsync(itemId);
            if (ti == null) return null;

            return new TaskItemDto
            {
                ItemID = ti.ItemID,
                FileName = ti.DataItem?.FileName ?? "Unknown File",
                FilePath = ti.DataItem?.FilePath ?? "",
                IsFlagged = ti.IsFlagged,
                Annotations = (ti.TaskItemDetails ?? new List<TaskItemDetail>()).Select(d => new AnnotationDetailDto
                {
                    AnnotationData = d.AnnotationData,
                    Content = d.Content,
                    Field = d.Field
                }).ToList()
            };
        }
    }
}