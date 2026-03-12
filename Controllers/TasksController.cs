using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using System.Security.Claims;

using TaskModel = SWP_BE.Models.Task;

namespace SWP_BE.Controllers
{
    /// <summary>
    /// PHÂN HỆ: TASK - Quản lý công việc gán nhãn cho Annotator & Reviewer
    /// </summary>
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [Role: Annotator, Reviewer] Lấy danh sách task được giao cho người dùng hiện tại.
        /// </summary>
        /// <remarks>
        /// - Annotator: Trả về các task mà mình được phân công gán nhãn.
        /// - Reviewer: Trả về các task mà mình được chỉ định kiểm duyệt.
        /// Danh sách được sắp xếp theo deadline tăng dần (ưu tiên task sắp hết hạn).
        /// </remarks>
        /// <response code="200">Trả về danh sách task.</response>
        /// <response code="401">Chưa đăng nhập hoặc không xác định được role.</response>
        [Authorize(Roles = "Annotator,Reviewer")]
        [HttpGet("my-tasks")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        [ProducesResponseType(401)]
        public async System.Threading.Tasks.Task<IActionResult> GetMyTasks()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdClaim == null || roleClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            // Lọc task theo role: Annotator xem task mình gán nhãn, Reviewer xem task mình kiểm duyệt
            IQueryable<TaskModel> tasksQuery = _context.Tasks;

            tasksQuery = roleClaim switch
            {
                "Annotator" => tasksQuery.Where(t => t.AnnotatorID == userId),
                "Reviewer" => tasksQuery.Where(t => t.ReviewerID == userId),
                _ => tasksQuery.Where(t => false)
            };

            var tasks = await tasksQuery
                .OrderBy(t => t.Deadline)
                .Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.Status,
                    t.Deadline,
                    t.ProjectID,
                    t.CurrentRound,
                    t.SubmissionRate,
                    t.RateComplete
                })
                .ToListAsync();

            return Ok(tasks);
        }
    }
}