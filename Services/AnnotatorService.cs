using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

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
                Guideline = t.Project?.GuidelineUrl ?? "",
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
                        Field = d.Field,
                        IsApproved = d.IsApproved
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

        public async System.Threading.Tasks.Task<bool> SaveAnnotation(Guid itemId, Guid userId, SaveAnnotationDto dto)
        {
            var item = await _repo.GetItemByIdAsync(itemId);
            if (item == null || item.Task == null ||
                item.Task.AnnotatorID != userId ||
                (item.Task.Status != SWP_BE.Models.Task.TaskStatus.InProgress &&
                item.Task.Status != SWP_BE.Models.Task.TaskStatus.Rejected))
            {
                return false;
            }

            try
            {
                if (item.TaskItemDetails != null && item.TaskItemDetails.Any())
                {
                    _repo.DeleteItemDetails(item.TaskItemDetails);
                }

                if (dto.Annotations != null)
                {
                    foreach (var ann in dto.Annotations)
                    {
                        // TRẠNG THÁI ĐƯỢC CHỐT Ở ĐÂY:
                        // - Nếu DTO mang chữ "New" (vừa vẽ) hoặc bị rỗng ➔ Ép lưu thành "Complete"
                        // - Nếu DTO mang chữ "True"/"False" (Đã bị chấm) ➔ Giữ nguyên không được đổi
                        // - Nếu DTO mang chữ "Complete" (Sửa lại khung) ➔ Vẫn là "Complete"
                        string finalStatus = ann.IsApproved;
                        if (string.IsNullOrEmpty(finalStatus) || finalStatus.Equals("New", StringComparison.OrdinalIgnoreCase))
                        {
                            finalStatus = "Complete";
                        }

                        item.TaskItemDetails.Add(new TaskItemDetail
                        {
                            AnnotationData = ann.AnnotationData,
                            Content = ann.Content,
                            Field = ann.Field,
                            TaskItemID = itemId,
                            IsApproved = finalStatus
                        });
                    }
                }
                await _repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lưu Database: {ex.Message}", ex);
            }
        }

        public async System.Threading.Tasks.Task<(bool Success, string Message)> SubmitTask(Guid taskId, Guid userId, bool isResubmit)
        {
            var task = await _repo.GetTaskByIdAsync(taskId, userId);
            if (task == null) return (false, "Task không tồn tại.");

            if (task.Status != SWP_BE.Models.Task.TaskStatus.InProgress &&
                task.Status != SWP_BE.Models.Task.TaskStatus.Rejected)
                return (false, "Bạn chỉ có thể nộp khi Task đang ở trạng thái InProgress.");

            if (isResubmit && task.CurrentRound >= 4)
                return (false, "Bạn đã sử dụng hết 3 lần sửa bài (Vòng 4 là cơ hội cuối cùng).");

            var items = task.TaskItems ?? new List<TaskItem>();
            if (items.Any(ti => !ti.IsFlagged && !(ti.TaskItemDetails?.Any() ?? false)))
                return (false, "Vui lòng hoàn thành gán nhãn cho tất cả các file trước khi nộp.");

            task.Status = SWP_BE.Models.Task.TaskStatus.PendingReview;
            task.CurrentRound++;

            await _repo.SaveChangesAsync();
            return (true, "Nộp bài thành công. Vui lòng đợi Reviewer phản hồi.");
        }

        public async System.Threading.Tasks.Task<bool> StartTask(Guid taskId, Guid userId)
        {
            var task = await _repo.GetTaskByIdAsync(taskId, userId);
            if (task == null) return false;
            if (task.Status == SWP_BE.Models.Task.TaskStatus.New || task.Status == SWP_BE.Models.Task.TaskStatus.Rejected)
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

            // Đóng băng Task bằng trạng thái Disputed
            task.Status = SWP_BE.Models.Task.TaskStatus.Disputed;

            await _repo.AddDisputeAsync(dispute);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async System.Threading.Tasks.Task<IEnumerable<object>> GetMyDisputes(Guid userId)
        {
            return await _repo.GetDisputesByUserIdAsync(userId);
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
                    Field = d.Field,
                    IsApproved = d.IsApproved
                }).ToList()
            };
        }
    }
}