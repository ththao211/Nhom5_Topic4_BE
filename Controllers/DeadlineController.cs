using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;

using TaskModel = SWP_BE.Models.Task;

namespace SWP_BE.Controllers
{
    /// <summary>
    /// PHÂN HỆ: DEADLINE - Quản lý thời hạn dự án và cảnh báo quá hạn
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api")]
    public class DeadlineController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeadlineController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [Role: Admin, Manager] Cập nhật deadline cho dự án.
        /// </summary>
        /// <param name="id">ID của dự án (Guid)</param>
        /// <param name="newDeadline">Thời hạn mới</param>
        /// <response code="200">Cập nhật deadline dự án thành công.</response>
        /// <response code="404">Dự án không tồn tại.</response>
        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("projects/{id}/deadline")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async System.Threading.Tasks.Task<IActionResult> UpdateProjectDeadline(Guid id, [FromBody] DateTime newDeadline)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound("Project not found");

            project.Deadline = newDeadline;
            await _context.SaveChangesAsync();
            return Ok("Project deadline updated successfully");
        }

        /// <summary>
        /// [Role: Admin, Manager] Lấy danh sách các task đã quá hạn trong dự án.
        /// </summary>
        /// <remarks>
        /// Trả về các task có deadline đã qua và chưa được Approved.
        /// Dùng để Manager theo dõi và quyết định thu hồi hoặc gia hạn task.
        /// </remarks>
        /// <param name="id">ID của dự án (Guid)</param>
        /// <response code="200">Trả về danh sách task quá hạn.</response>
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("projects/{id}/overdue-tasks")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async System.Threading.Tasks.Task<IActionResult> GetOverdueTasks(Guid id)
        {
            var now = DateTime.UtcNow;

            var overdueTasks = await _context.Tasks
                .Where(t => t.ProjectID == id
                            && t.Deadline < now
                            && t.Status != "Approved")
                .Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.Status,
                    t.Deadline,
                    t.AnnotatorID,
                    t.ReviewerID,
                    t.RejectCount
                })
                .ToListAsync();

            return Ok(overdueTasks);
        }
    }
}