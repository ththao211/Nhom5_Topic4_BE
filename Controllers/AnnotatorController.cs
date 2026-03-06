using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP_BE.DTOs;
using SWP_BE.Services;
using System;
using System.Security.Claims;

namespace SWP_BE.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class AnnotatorController : ControllerBase
    {
        private readonly AnnotatorService _service;

        public AnnotatorController(AnnotatorService service) { _service = service; }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId)) return userId;
            return Guid.Empty;
        }

        /// <summary>
        /// 1. Lấy danh sách Task của Annotator
        /// </summary>
        /// <remarks>
        /// Trạng thái (status) có thể truyền: "New", "InProgress", "PendingReview"...
        /// Nếu không truyền sẽ lấy toàn bộ.
        /// </remarks>
        [HttpGet("annotator/tasks")]
        public async System.Threading.Tasks.Task<IActionResult> GetTasks(string? status)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized("Token không hợp lệ.");
            return Ok(await _service.GetTasks(userId, status));
        }

        /// <summary>
        /// 2. Lấy toàn bộ không gian làm việc (Workspace) của 1 Task
        /// </summary>
        /// <remarks>
        /// Trả về danh sách ảnh và toàn bộ tọa độ cũ (nếu có) để FE vẽ lên Canvas.
        /// </remarks>
        [HttpGet("annotator/tasks/{taskId}")]
        public async System.Threading.Tasks.Task<IActionResult> GetDetail(Guid taskId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized("Token không hợp lệ.");
            var detail = await _service.GetTaskDetail(taskId, userId);
            if (detail == null) return NotFound("Task không tồn tại.");
            return Ok(detail);
        }

        /// <summary>
        /// 3. Bấm nút "Bắt đầu làm" trên giao diện
        /// </summary>
        /// <remarks>
        /// Đổi trạng thái Task từ New -> InProgress để bắt đầu gán nhãn.
        /// </remarks>
        [HttpPatch("annotator/tasks/{taskId}/start")]
        public async System.Threading.Tasks.Task<IActionResult> Start(Guid taskId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var result = await _service.StartTask(taskId, userId);
            return result ? Ok(new { message = "Đã bắt đầu làm Task" }) : BadRequest("Không thể bắt đầu task.");
        }

        /// <summary>
        /// 4. LƯU TỌA ĐỘ VẼ (Gọi nhiều nhất)
        /// </summary>
        /// <remarks>
        /// Truyền vào mảng tọa độ JSON. Dùng để lưu nháp liên tục khi đang vẽ trên Canvas.
        /// </remarks>
        [HttpPost("task-items/{itemId}/annotation")]
        public async System.Threading.Tasks.Task<IActionResult> Save(Guid itemId, [FromBody] SaveAnnotationDto dto)
        {
            var result = await _service.SaveAnnotation(itemId, dto);
            return result ? Ok(new { message = "Lưu thành công" }) : BadRequest("Lưu thất bại.");
        }

        /// <summary>
        /// 4.1 Lấy tọa độ gán nhãn của 1 tấm ảnh duy nhất
        /// </summary>
        /// <remarks>
        /// Dùng để tối ưu hiệu năng (Lazy Loading). Khi click vào ảnh nào mới gọi API lấy tọa độ ảnh đó.
        /// </remarks>
        [HttpGet("task-items/{itemId}")]
        public async System.Threading.Tasks.Task<IActionResult> GetItem(Guid itemId)
        {
            var item = await _service.GetItemDetail(itemId);
            if (item == null) return NotFound("Không tìm thấy ảnh hoặc dữ liệu.");
            return Ok(item);
        }

        /// <summary>
        /// 5. Báo lỗi 1 tấm ảnh
        /// </summary>
        /// <remarks>
        /// Dùng khi ảnh bị đen, mờ, hỏng... không thể gán nhãn được.
        /// </remarks>
        [HttpPatch("task-items/{itemId}/flag")]
        public async System.Threading.Tasks.Task<IActionResult> Flag(Guid itemId)
        {
            var result = await _service.FlagItem(itemId);
            return result ? Ok(new { message = "Đã đánh dấu ảnh lỗi" }) : BadRequest("Thao tác thất bại.");
        }

        /// <summary>
        /// 6. Nộp bài lần đầu
        /// </summary>
        /// <remarks>
        /// Chỉ nộp được khi tất cả các ảnh (không bị Flag) đã có tọa độ gán nhãn.
        /// </remarks>
        [HttpPost("tasks/{taskId}/submit")]
        public async System.Threading.Tasks.Task<IActionResult> Submit(Guid taskId)
        {
            var userId = GetCurrentUserId();
            var result = await _service.SubmitTask(taskId, userId, false);
            return result.Success ? Ok(result.Message) : BadRequest(result.Message);
        }

        /// <summary>
        /// 7. Nộp lại bài (Resubmit)
        /// </summary>
        /// <remarks>
        /// Dùng sau khi bị Reviewer bắt sửa lại. Tối đa nộp lại 3 lần cho mỗi Task.
        /// </remarks>
        [HttpPost("tasks/{taskId}/resubmit")]
        public async System.Threading.Tasks.Task<IActionResult> Resubmit(Guid taskId)
        {
            var userId = GetCurrentUserId();
            var result = await _service.SubmitTask(taskId, userId, true);
            return result.Success ? Ok(result.Message) : BadRequest(result.Message);
        }

        /// <summary>
        /// 8. Gửi khiếu nại (Dispute)
        /// </summary>
        /// <remarks>
        /// Dùng khi bị Reviewer đánh rớt nhưng Annotator thấy mình làm đúng.
        /// </remarks>
        [HttpPost("tasks/{taskId}/dispute")]
        public async System.Threading.Tasks.Task<IActionResult> Dispute(Guid taskId, [FromBody] DisputeRequestDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _service.CreateDispute(taskId, userId, dto);
            return result ? Ok(new { message = "Đã gửi khiếu nại" }) : BadRequest("Gửi khiếu nại thất bại.");
        }

        /// <summary>
        /// 9. Lấy thông tin điểm tín nhiệm (Reputation)
        /// </summary>
        /// <remarks>
        /// Trả về tổng điểm hiện tại và lịch sử biến động điểm của User.
        /// </remarks>
        [HttpGet("annotator/reputation")]
        public async System.Threading.Tasks.Task<IActionResult> GetRep()
        {
            var userId = GetCurrentUserId();
            var data = await _service.GetReputation(userId);
            return data != null ? Ok(data) : NotFound("Không thấy dữ liệu.");
        }
    }
}