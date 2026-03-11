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
using Microsoft.AspNetCore.Http; // Thêm thư viện này để dùng các StatusCodes cho Swagger

namespace SWP_BE.Controllers
{
    [Route("api/reviewer")]
    [ApiController]
    [Authorize(Roles = "Reviewer")]
    [Tags("Reviewer Task Management")] // Thêm Tag để nhóm các API này trong Swagger
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
        // 1. LẤY DANH SÁCH TASK (Mặc định lấy ALL, có thể lọc theo Status)
        // ============================================================

        /// <summary>
        /// Lấy danh sách các Task được giao cho Reviewer hiện tại.
        /// </summary>
        /// <remarks>
        /// Mặc định sẽ trả về toàn bộ Task của Reviewer. Có thể truyền thêm tham số status để lọc.
        /// </remarks>
        /// <param name="status">Trạng thái của Task cần lọc (vd: PendingReview, Approved, InProgress). Bỏ trống để lấy tất cả.</param>
        /// <returns>Danh sách các Task của Reviewer.</returns>
        [HttpGet("tasks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Lấy thông tin chi tiết của một Task để duyệt.
        /// </summary>
        /// <remarks>
        /// Trả về toàn bộ dữ liệu của Task bao gồm các DataItem bên trong và chi tiết tọa độ gán nhãn của Annotator.
        /// Chỉ những Task ở trạng thái PendingReview (Chờ duyệt) mới được phép xem chi tiết.
        /// </remarks>
        /// <param name="taskId">Mã Guid của Task cần xem.</param>
        /// <returns>Thông tin chi tiết Task và dữ liệu gán nhãn.</returns>
        [HttpGet("tasks/{taskId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Đánh giá (Đúng/Sai) cho từng nhãn (Annotation) bên trong Task.
        /// </summary>
        /// <remarks>
        /// Dùng để Reviewer tick chọn xem một nhãn cụ thể mà Annotator đã làm là đúng hay sai.
        /// </remarks>
        /// <param name="id">Mã ID của chi tiết nhãn (TaskItemDetail ID).</param>
        /// <param name="isApproved">Trạng thái đánh giá: true (Đúng), false (Sai).</param>
        /// <returns>Thông báo kết quả đánh giá.</returns>
        [HttpPatch("item-detail/{id}/check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReviewItemDetail(int id, [FromQuery] bool isApproved)
        {
            var detail = await _context.TaskItemDetails.FindAsync(id);
            if (detail == null) return NotFound();

            detail.IsApproved = isApproved;

            await _context.SaveChangesAsync();

            // UPDATE PROGRESS
            var taskId = await _context.TaskItems
                .Where(i => i.ItemID == detail.TaskItemID)
                .Select(i => i.TaskID)
                .FirstOrDefaultAsync();

            await _progressService.UpdateTaskAndProject(taskId);

            return Ok(new { message = isApproved ? "Đã đánh dấu ĐÚNG" : "Đã đánh dấu SAI" });
        }

        // ============================================================
        // 4. APPROVE (Duyệt toàn bộ Task)
        // ============================================================

        /// <summary>
        /// Chấp nhận (Approve) toàn bộ Task.
        /// </summary>
        /// <remarks>
        /// Đánh dấu Task là Approved. Hệ thống sẽ tự động cập nhật điểm tín nhiệm cho Annotator và gửi thông báo.
        /// </remarks>
        /// <param name="taskId">Mã Guid của Task cần duyệt.</param>
        /// <returns>Thông báo duyệt thành công.</returns>
        [HttpPost("tasks/{taskId}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            await _progressService.UpdateTaskAndProject(task.TaskID);
            return Ok("Task Approved và đã cập nhật điểm tín nhiệm.");
        }

        // ============================================================
        // 5. REJECT (Từ chối Task)
        // ============================================================

        /// <summary>
        /// Từ chối (Reject) Task và yêu cầu Annotator sửa lại.
        /// </summary>
        /// <remarks>
        /// Nếu số lần Reject &lt; 4: Task sẽ quay lại trạng thái InProgress cho Annotator sửa.<br/>
        /// Nếu Reject đến lần thứ 4: Task chính thức bị đánh FAIL, Annotator bị trừ điểm tín nhiệm.
        /// </remarks>
        /// <param name="taskId">Mã Guid của Task cần từ chối.</param>
        /// <param name="feedback">Lý do từ chối (bắt buộc nhập).</param>
        /// <returns>Trạng thái sau khi Reject.</returns>
        [HttpPost("tasks/{taskId}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            task.Status = SWP_BE.Models.Task.TaskStatus.InProgress;

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
            // UPDATE PROGRESS
            await _progressService.UpdateTaskAndProject(task.TaskID);
            return Ok(task.Status == SWP_BE.Models.Task.TaskStatus.Fail ? "Task đã bị đánh FAIL" : $"Task bị Reject lần {task.RejectCount}");
        }

        /// <summary>
        /// Hàm nội bộ: Lấy ID của Reviewer đang đăng nhập.
        /// </summary>
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