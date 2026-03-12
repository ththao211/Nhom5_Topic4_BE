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

        // ============================================================
        // 1. LẤY DANH SÁCH TASK CỦA ANNOTATOR
        // ============================================================
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

        // ============================================================
        // 2. XEM CHI TIẾT TASK (BAO GỒM CÁC FILE VÀ NHÃN)
        // ============================================================
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

        // ============================================================
        // 3. LƯU TỌA ĐỘ/NỘI DUNG GÁN NHÃN (LƯU TẠM)
        // ============================================================
        public async System.Threading.Tasks.Task<bool> SaveAnnotation(Guid itemId, Guid userId, SaveAnnotationDto dto)
        {
            var item = await _repo.GetItemByIdAsync(itemId);
            // 1. Phải đúng là người được giao Task (AnnotatorID == userId)
            // 2. Task phải đang trong trạng thái được phép sửa (Status == InProgress)
            if (item == null || item.Task == null ||
                item.Task.AnnotatorID != userId ||
                item.Task.Status != SWP_BE.Models.Task.TaskStatus.InProgress)
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
                        item.TaskItemDetails.Add(new TaskItemDetail
                        {
                            AnnotationData = ann.AnnotationData,
                            Content = ann.Content,
                            Field = ann.Field,
                            TaskItemID = itemId
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

        // ============================================================
        // 4. NỘP BÀI (SUBMIT) - TĂNG VÒNG NỘP VÀ ĐỢI REVIEW
        // ============================================================
        public async System.Threading.Tasks.Task<(bool Success, string Message)> SubmitTask(Guid taskId, Guid userId, bool isResubmit)
        {
            var task = await _repo.GetTaskByIdAsync(taskId, userId);
            if (task == null) return (false, "Task không tồn tại.");

            // Chỉ cho phép nộp khi Task đang ở trạng thái làm việc
            if (task.Status != SWP_BE.Models.Task.TaskStatus.InProgress)
                return (false, "Bạn chỉ có thể nộp khi Task đang ở trạng thái InProgress.");

            // Kiểm tra giới hạn: Nếu đã nộp 3 lần (vòng 4 bị Reject) thì không được nộp tiếp
            if (isResubmit && task.CurrentRound >= 4)
                return (false, "Bạn đã sử dụng hết 3 lần sửa bài (Vòng 4 là cơ hội cuối cùng).");

            // Kiểm tra xem tất cả các file đã được gán nhãn hoặc báo lỗi (Flag) chưa
            var items = task.TaskItems ?? new List<TaskItem>();
            if (items.Any(ti => !ti.IsFlagged && !(ti.TaskItemDetails?.Any() ?? false)))
                return (false, "Vui lòng hoàn thành gán nhãn cho tất cả các file trước khi nộp.");

            // Chuyển sang trạng thái chờ duyệt
            task.Status = SWP_BE.Models.Task.TaskStatus.PendingReview;

            // LUÔN TĂNG VÒNG NỘP: Nộp lần đầu = 1, Lần sửa 1 = 2...
            task.CurrentRound++;

            await _repo.SaveChangesAsync();
            return (true, "Nộp bài thành công. Vui lòng đợi Reviewer phản hồi.");
        }

        // ============================================================
        // 5. BẮT ĐẦU LÀM (START) - HỖ TRỢ CẢ NEW VÀ REJECTED
        // ============================================================
        public async System.Threading.Tasks.Task<bool> StartTask(Guid taskId, Guid userId)
        {
            var task = await _repo.GetTaskByIdAsync(taskId, userId);
            if (task == null) return false;
            // Chấp nhận chuyển sang InProgress từ trạng thái Mới (New) hoặc bị Trả về (Rejected)
            if (task.Status == SWP_BE.Models.Task.TaskStatus.New || task.Status == SWP_BE.Models.Task.TaskStatus.Rejected)
            {
                task.Status = SWP_BE.Models.Task.TaskStatus.InProgress;
                await _repo.SaveChangesAsync();
            }
            return true;
        }

        // ============================================================
        // 6. BÁO LỖI FILE (FLAG) - TRƯỜNG HỢP ẢNH LỖI, KHÔNG GÁN NHÃN ĐC
        // ============================================================
        public async System.Threading.Tasks.Task<bool> FlagItem(Guid itemId)
        {
            var item = await _repo.GetItemByIdAsync(itemId);
            if (item == null) return false;
            item.IsFlagged = true;
            await _repo.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // 7. KHIẾU NẠI (DISPUTE) - KHI KHÔNG ĐỒNG Ý VỚI REVIEWER
        // ============================================================
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

        // ============================================================
        // 8. XEM ĐIỂM TÍN NHIỆM VÀ LỊCH SỬ BIẾN ĐỘNG
        // ============================================================
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

        // ============================================================
        // 9. LẤY CHI TIẾT 1 TẤM ẢNH 
        // ============================================================
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