using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SWP_BE.Controllers
{
    [Route("api/reviewer")]
    [ApiController]
    [Authorize(Roles = "Reviewer")]
    [Tags("Reviewer Task Management")]
    public class ReviewerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ReputationService _reputationService;
        private readonly IProgressService _progressService;

        public ReviewerController(
            AppDbContext context,
            ReputationService reputationService,
            INotificationService notificationService,
            IProgressService progressService)
        {
            _context = context;
            _reputationService = reputationService;
            _notificationService = notificationService;
            _progressService = progressService;
        }

        // ============================================================
        // 1. LẤY DANH SÁCH TASK CỦA REVIEWER
        // ============================================================
        [HttpGet("tasks")]
        public async Task<IActionResult> GetReviewerTasks([FromQuery] string? status)
        {
            var reviewerId = GetCurrentUserId();
            var query = _context.Tasks.Where(t => t.ReviewerID == reviewerId);

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<SWP_BE.Models.Task.TaskStatus>(status, true, out var taskStatus))
                {
                    query = query.Where(t => t.Status == taskStatus);
                }
            }

            var tasks = await query
                .Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.ProjectID,
                    Status = t.Status.ToString(),
                    t.Deadline,
                    t.CurrentRound
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // ============================================================
        // 2. XEM CHI TIẾT TASK ĐỂ CHẤM ĐIỂM
        // ============================================================
        [HttpGet("tasks/{taskId}")]
        public async Task<IActionResult> GetTaskDetail(Guid taskId)
        {
            var reviewerId = GetCurrentUserId();

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.TaskItems).ThenInclude(i => i.DataItem)
                .Include(t => t.TaskItems).ThenInclude(i => i.TaskItemDetails)
                .FirstOrDefaultAsync(t => t.TaskID == taskId && t.ReviewerID == reviewerId);

            if (task == null) return NotFound("Không tìm thấy Task.");

            return Ok(new
            {
                task.TaskID,
                task.TaskName,
                ProjectName = task.Project?.ProjectName,
                Status = task.Status.ToString(),
                task.CurrentRound,
                task.RateComplete,
                Items = task.TaskItems.Select(i => new {
                    i.ItemID,
                    i.DataItem.FilePath,
                    i.DataItem.FileName,
                    Annotations = i.TaskItemDetails.Select(d => new {
                        d.IDDetail,
                        d.AnnotationData,
                        d.Content,
                        d.Field,
                        d.IsApproved
                    })
                })
            });
        }

        // ============================================================
        // 3. DUYỆT TỪNG NHÃN (ĐÚNG/SAI)
        // ============================================================
        [HttpPatch("item-detail/{id}/check")]
        public async Task<IActionResult> ReviewItemDetail(int id, [FromQuery] bool isApproved)
        {
            var detail = await _context.TaskItemDetails.FindAsync(id);
            if (detail == null) return NotFound();

            detail.IsApproved = isApproved;
            await _context.SaveChangesAsync();

            var taskId = await _context.TaskItems
                .Where(i => i.ItemID == detail.TaskItemID)
                .Select(i => i.TaskID)
                .FirstOrDefaultAsync();

            // Cập nhật RateComplete thực tế của Task ngay lập tức
            await _progressService.UpdateTaskAndProject(taskId);

            return Ok(new { message = isApproved ? "Đã đánh dấu ĐÚNG" : "Đã đánh dấu SAI" });
        }

        // ============================================================
        // 4. APPROVE (CHỐT DUYỆT TASK)
        // ============================================================
        [HttpPost("tasks/{taskId}/approve")]
        public async Task<IActionResult> Approve(Guid taskId)
        {
            var reviewerId = GetCurrentUserId();
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskID == taskId && t.ReviewerID == reviewerId);

            if (task == null || task.Status != SWP_BE.Models.Task.TaskStatus.PendingReview)
                return BadRequest("Thao tác không hợp lệ.");

            // Chụp lại tỷ lệ lần đầu nếu Approve ngay vòng 1 để làm bằng chứng thưởng 2đ
            if (task.CurrentRound == 1) task.FirstRate = task.RateComplete;

            task.Status = SWP_BE.Models.Task.TaskStatus.Approved;
            task.CompletedAt = DateTime.UtcNow;

            if (task.AnnotatorID.HasValue)
            {
                await _reputationService.HandleTaskCompletionAsync(task.AnnotatorID.Value, task);
                await _notificationService.NotifyTaskApproved(task.AnnotatorID.Value, task.TaskName);
            }

            await _context.SaveChangesAsync();
            await _progressService.UpdateTaskAndProject(task.TaskID);
            return Ok("Task Approved thành công.");
        }

        // ============================================================
        // 5. REJECT (TỪ CHỐI BÀI LÀM)
        // ============================================================
        [HttpPost("tasks/{taskId}/reject")]
        public async Task<IActionResult> Reject(Guid taskId, [FromBody] FeedbackDTO feedback)
        {
            var reviewerId = GetCurrentUserId();

            var task = await _context.Tasks
                .Include(t => t.TaskItems)
                .FirstOrDefaultAsync(t => t.TaskID == taskId && t.ReviewerID == reviewerId);

            if (task == null || task.Status != SWP_BE.Models.Task.TaskStatus.PendingReview)
                return BadRequest("Task không ở trạng thái chờ duyệt.");

            if (string.IsNullOrWhiteSpace(feedback.Comment))
                return BadRequest("Vui lòng nhập lý do từ chối.");

            // Chụp lại tỷ lệ vòng 1 khi bị Reject lần đầu
            if (task.CurrentRound == 1) task.FirstRate = task.RateComplete;

            // Nếu đã tới lượt nộp thứ 4 mà vẫn sai -> Đánh FAIL và tạo Task mới cho Manager
            if (task.CurrentRound == 4)
            {
                task.Status = SWP_BE.Models.Task.TaskStatus.Fail;
                task.CompletedAt = DateTime.UtcNow;

                if (task.AnnotatorID.HasValue)
                {
                    await _reputationService.HandleTaskCompletionAsync(task.AnnotatorID.Value, task);
                    await _notificationService.NotifyTaskRejected(task.AnnotatorID.Value, task.TaskName, "Task bị FAIL do sai quá 3 lần.");
                }

                // TẠO BẢN SAO MỚI (Reset mọi thông số cho người mới)
                var clonedTask = new SWP_BE.Models.Task
                {
                    TaskID = Guid.NewGuid(),
                    TaskName = task.TaskName,
                    ProjectID = task.ProjectID,
                    Status = SWP_BE.Models.Task.TaskStatus.New,
                    CurrentRound = 0,
                    RateComplete = 0,
                    FirstRate = null,
                    AnnotatorID = null, // Manager sẽ gán người mới
                    ReviewerID = null,  // Manager sẽ gán reviewer mới
                    Deadline = DateTime.UtcNow.AddDays(3),
                    CreatedAt = DateTime.UtcNow,

                    // Sửa lỗi: Dùng đúng tên biến DataID từ Model TaskItem của bạn
                    TaskItems = task.TaskItems.Select(ti => new TaskItem
                    {
                        ItemID = Guid.NewGuid(),
                        DataID = ti.DataID,
                        IsFlagged = false,
                        // Khởi tạo List rỗng để AnnotatorService.SaveAnnotation không bị lỗi Null
                        TaskItemDetails = new List<TaskItemDetail>()
                    }).ToList()
                };
                _context.Tasks.Add(clonedTask);
            }
            else
            {
                // Trả về trạng thái Rejected để Annotator sửa tiếp
                task.Status = SWP_BE.Models.Task.TaskStatus.Rejected;

                if (task.AnnotatorID.HasValue)
                {
                    await _notificationService.NotifyTaskRejected(task.AnnotatorID.Value, task.TaskName, feedback.Comment);
                }
            }

            await _context.SaveChangesAsync();
            await _progressService.UpdateTaskAndProject(task.TaskID);

            return Ok(task.Status == SWP_BE.Models.Task.TaskStatus.Fail ? "Task đã bị đánh FAIL" : "Task đã được chuyển về trạng thái REJECTED");
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(userIdStr, out Guid userId) ? userId : Guid.Empty;
        }
    }
}