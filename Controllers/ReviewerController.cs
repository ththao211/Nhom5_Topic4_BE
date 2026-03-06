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

namespace SWP_BE.Controllers
{
    [Route("api/reviewer")]
    [ApiController]
    [Authorize(Roles = "Reviewer")]
    public class ReviewerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ReputationService _reputationService;

        public ReviewerController(
            AppDbContext context,
            ReputationService reputationService,
            INotificationService notificationService)
        {
            _context = context;
            _reputationService = reputationService;
            _notificationService = notificationService;
        }

        // ============================================================
        // 1. LẤY DANH SÁCH TASK (Mặc định lấy ALL, có thể lọc theo Status)
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
                else
                {
                    return BadRequest($"Trạng thái '{status}' không hợp lệ.");
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

            detail.IsApproved = isApproved;
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

            // Cập nhật trạng thái terminal (Kết thúc)
            task.Status = SWP_BE.Models.Task.TaskStatus.Approved;
            task.CompletedAt = DateTime.UtcNow;

            if (task.AnnotatorID.HasValue)
            {
                // GỌI SERVICE: Tự động check RejectCount để cộng +20, -5, hoặc thưởng +2...
                await _reputationService.HandleTaskCompletionAsync(task.AnnotatorID.Value, task);

                await _notificationService.NotifyTaskApproved(task.AnnotatorID.Value, task.TaskName);
            }

            await _context.SaveChangesAsync();
            return Ok("Task Approved và đã cập nhật điểm tín nhiệm.");
        }

        // ============================================================
        // 5. REJECT (Từ chối Task)
        // ============================================================
        [HttpPost("tasks/{taskId}/reject")]
        public async Task<IActionResult> Reject(Guid taskId, [FromBody] FeedbackDTO feedback)
        {
            var reviewerId = GetCurrentUserId();
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskID == taskId && t.ReviewerID == reviewerId);

            if (task == null || task.Status != SWP_BE.Models.Task.TaskStatus.PendingReview)
                return BadRequest("Task không hợp lệ.");

            if (string.IsNullOrWhiteSpace(feedback.Comment))
                return BadRequest("Vui lòng nhập lý do từ chối.");

            // Tăng số lần Reject lên
            task.RejectCount++;

            // KIỂM TRA: Nếu đây là lần Reject thứ 4 -> Task chính thức FAIL
            if (task.RejectCount >= 4)
            {
                task.Status = SWP_BE.Models.Task.TaskStatus.Fail;
                task.CompletedAt = DateTime.UtcNow;

                if (task.AnnotatorID.HasValue)
                {
                    // GỌI SERVICE: Trừ -20đ và kiểm tra xem có bị khóa tài khoản không
                    await _reputationService.HandleTaskCompletionAsync(task.AnnotatorID.Value, task);

                    await _notificationService.NotifyTaskRejected(task.AnnotatorID.Value, task.TaskName, "Task bị FAIL do vượt quá 3 lần sửa.");
                }
            }
            else
            {
                // Nếu chưa tới 4 lần thì trả về cho Annotator sửa tiếp
                task.Status = SWP_BE.Models.Task.TaskStatus.InProgress;

                if (task.AnnotatorID.HasValue)
                {
                    await _notificationService.NotifyTaskRejected(task.AnnotatorID.Value, task.TaskName, feedback.Comment);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(task.Status == SWP_BE.Models.Task.TaskStatus.Fail ? "Task đã bị đánh FAIL" : $"Task bị Reject lần {task.RejectCount}");
        }

        private Guid GetCurrentUserId()
        {
            // Cập nhật để đọc đúng Token mới
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ hoặc thiếu ID.");
            }
            return userId;
        }
    }
}