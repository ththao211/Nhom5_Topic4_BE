using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Services;
using System.Security.Claims;

namespace SWP_BE.Controllers
{
    [Route("api/reviewer")]
    [ApiController]
    [Authorize(Roles = "Reviewer")]
    public class ReviewerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public ReviewerController(
            AppDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // ============================================================
        // 1. LẤY DANH SÁCH TASK ĐANG CHỜ DUYỆT (Status = PendingReview)
        // ============================================================
        [HttpGet("tasks/pending")]
        public async Task<IActionResult> GetPendingTasks()
        {
            var reviewerId = GetCurrentUserId();

            // FIX CS0019: So sánh trực tiếp với giá trị Enum TaskStatus
            var tasks = await _context.Tasks
                .Where(t => t.ReviewerID == reviewerId &&
                            t.Status == SWP_BE.Models.Task.TaskStatus.PendingReview)
                .Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.ProjectID,
                    t.Deadline,
                    t.CurrentRound,
                    t.RejectCount
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // ============================================================
        // 2. XEM CHI TIẾT TASK (Bao gồm các Item và tọa độ Annotator đã vẽ)
        // ============================================================
        [HttpGet("tasks/{taskId}")]
        public async Task<IActionResult> GetTaskDetail(Guid taskId)
        {
            var reviewerId = GetCurrentUserId();

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.TaskItems)
                    .ThenInclude(i => i.DataItem)
                .Include(t => t.TaskItems)
                    .ThenInclude(i => i.TaskItemDetails)
                .FirstOrDefaultAsync(t =>
                    t.TaskID == taskId &&
                    t.ReviewerID == reviewerId);

            if (task == null) return NotFound("Không tìm thấy Task.");

            // FIX CS0019: So sánh Enum
            if (task.Status != SWP_BE.Models.Task.TaskStatus.PendingReview)
                return BadRequest("Task không ở trạng thái chờ duyệt.");

            return Ok(new
            {
                task.TaskID,
                task.TaskName,
                ProjectName = task.Project?.ProjectName,
                Status = task.Status.ToString(),
                task.CurrentRound,
                task.RejectCount,
                // Trả về danh sách các item để Reviewer kiểm tra đúng/sai
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
        // 3. CHECK ĐÚNG/SAI TỪNG DATA TRONG 1 TASK
        // ============================================================
        [HttpPatch("item-detail/{id}/check")]
        public async Task<IActionResult> ReviewItemDetail(int id, [FromQuery] bool isApproved)
        {
            var detail = await _context.TaskItemDetails.FindAsync(id);
            if (detail == null) return NotFound();

            detail.IsApproved = isApproved; // Lưu kết quả kiểm tra từng khung hình
            await _context.SaveChangesAsync();

            return Ok(new { message = isApproved ? "Đã đánh dấu ĐÚNG" : "Đã đánh dấu SAI" });
        }

        // ============================================================
        // 4. APPROVE (Duyệt toàn bộ Task)
        // ============================================================
        [HttpPost("tasks/{taskId}/approve")]
        public async Task<IActionResult> Approve(Guid taskId)
        {
            var reviewerId = GetCurrentUserId();
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskID == taskId && t.ReviewerID == reviewerId);

            if (task == null || task.Status != SWP_BE.Models.Task.TaskStatus.PendingReview)
                return BadRequest("Thao tác không hợp lệ.");

            task.Status = SWP_BE.Models.Task.TaskStatus.Approved;
            task.CompletedAt = DateTime.UtcNow;

            if (task.AnnotatorID.HasValue)
            {
                _context.ReputationLogs.Add(new ReputationLog
                {
                    UserID = task.AnnotatorID.Value,
                    ScoreChange = 10,
                    Reason = $"Task {task.TaskName} Approved",
                    TaskID = task.TaskID,
                    CreatedAt = DateTime.UtcNow
                });
                await _notificationService.NotifyTaskApproved(task.AnnotatorID.Value, task.TaskName);
            }

            await _context.SaveChangesAsync();
            return Ok("Task Approved");
        }

        // ============================================================
        // 5. REJECT (Từ chối - Trả về bắt làm lại)
        // ============================================================
        [HttpPost("tasks/{taskId}/reject")]
        public async Task<IActionResult> Reject(Guid taskId, [FromBody] string reason)
        {
            var reviewerId = GetCurrentUserId();
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskID == taskId && t.ReviewerID == reviewerId);

            if (task == null || task.Status != SWP_BE.Models.Task.TaskStatus.PendingReview)
                return BadRequest();

            task.RejectCount++;

            // FIX CS0117: Vì Model không có trạng thái PendingRework hay Failed, 
            // ta đưa về InProgress để Annotator sửa bài
            task.Status = SWP_BE.Models.Task.TaskStatus.InProgress;

            if (task.AnnotatorID.HasValue)
            {
                _context.ReputationLogs.Add(new ReputationLog
                {
                    UserID = task.AnnotatorID.Value,
                    ScoreChange = -5,
                    Reason = $"Reject lần {task.RejectCount}: {reason}",
                    TaskID = task.TaskID,
                    CreatedAt = DateTime.UtcNow
                });
                await _notificationService.NotifyTaskRejected(task.AnnotatorID.Value, task.TaskName, reason);
            }

            await _context.SaveChangesAsync();
            return Ok("Task Rejected");
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("id")?.Value;
            return Guid.Parse(userId);
        }
    }
}