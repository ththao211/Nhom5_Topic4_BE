using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.Models;
using System.Security.Claims;

using TaskModel = SWP_BE.Models.Task;

namespace SWP_BE.Controllers
{
    /// <summary>
    /// PHÂN HỆ: THỐNG KÊ DỰ ÁN - Dashboard, hiệu suất, xuất dữ liệu & điểm tín nhiệm
    /// </summary>
    [ApiController]
    [Route("api/projects")]
    public class ProjectStatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectStatsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [Role: Admin, Manager] Lấy thống kê tổng quan của một dự án.
        /// </summary>
        /// <remarks>
        /// Trả về tổng số task, số lượng Approved/Pending/Rejected và tỷ lệ hoàn thành (%).
        /// </remarks>
        /// <param name="projectId">ID của dự án (Guid)</param>
        /// <response code="200">Trả về thống kê dự án.</response>
        /// <response code="404">Không tìm thấy task nào trong dự án.</response>
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{projectId}/statistics")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async System.Threading.Tasks.Task<IActionResult> GetProjectStatistics(Guid projectId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.ProjectID == projectId)
                .ToListAsync();

            if (!tasks.Any())
                return NotFound("No tasks found");

            var total = tasks.Count;
            var approved = tasks.Count(t => t.Status == "Approved");
            var pending = tasks.Count(t => t.Status == "Pending");
            var rejected = tasks.Count(t => t.Status == "Rejected");

            double rateComplete = total == 0 ? 0 : (double)approved / total * 100;

            return Ok(new
            {
                TotalTasks = total,
                Approved = approved,
                Pending = pending,
                Rejected = rejected,
                RateComplete = Math.Round(rateComplete, 2)
            });
        }

        /// <summary>
        /// [Role: Admin, Manager] Lấy hiệu suất làm việc của từng Annotator trong dự án.
        /// </summary>
        /// <remarks>
        /// Thống kê theo từng Annotator: tổng task, số Approved/Rejected, tỷ lệ reject (%),
        /// và thời gian xử lý trung bình (giờ).
        /// </remarks>
        /// <param name="projectId">ID của dự án (Guid)</param>
        /// <response code="200">Trả về danh sách hiệu suất từng Annotator.</response>
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{projectId}/user-performance")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async System.Threading.Tasks.Task<IActionResult> GetUserPerformance(Guid projectId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.ProjectID == projectId)
                .ToListAsync();

            var performance = tasks
                .GroupBy(t => t.AnnotatorID)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalTasks = g.Count(),
                    ApprovedTasks = g.Count(x => x.Status == "Approved"),
                    RejectedTasks = g.Count(x => x.Status == "Rejected"),

                    RejectRate = g.Count() == 0 ? 0 :
                        (double)g.Count(x => x.Status == "Rejected") / g.Count() * 100,

                    AvgProcessingTimeHours = g
                        .Where(x => x.CompletedAt.HasValue)
                        .Any()
                        ? g.Where(x => x.CompletedAt.HasValue)
                           .Average(x => (x.CompletedAt!.Value - x.CreatedAt).TotalHours)
                        : 0
                });

            return Ok(performance);
        }

        /// <summary>
        /// [Role: Manager] Xuất dữ liệu đã duyệt (Approved) của dự án.
        /// </summary>
        /// <remarks>
        /// Chỉ export các task có trạng thái Approved. Hệ thống lưu lại lịch sử export.
        /// Định dạng hỗ trợ: YOLO, VOC, JSON, CSV.
        /// </remarks>
        /// <param name="projectId">ID của dự án (Guid)</param>
        /// <param name="request">Thông tin định dạng xuất</param>
        /// <response code="200">Export thành công.</response>
        /// <response code="400">Không có task Approved nào để export.</response>
        /// <response code="404">Dự án không tồn tại.</response>
        [Authorize(Roles = "Manager")]
        [HttpPost("{projectId}/exports")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async System.Threading.Tasks.Task<IActionResult> ExportProject(Guid projectId, [FromBody] ExportRequest request)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound("Project not found");

            var approvedTasks = await _context.Tasks
                .Where(t => t.ProjectID == projectId && t.Status == "Approved")
                .ToListAsync();

            if (!approvedTasks.Any())
                return BadRequest("No approved tasks to export");

            var managerIdClaim = User.FindFirst("id")?.Value;
            if (managerIdClaim == null)
                return Unauthorized();

            var exportHistory = new ExportHistory
            {
                ExportID = Guid.NewGuid(),
                Format = request.Formats.ToUpper(),
                ItemCount = approvedTasks.Count,
                CreatedAt = DateTime.UtcNow,
                ProjectID = projectId,
                ManagerID = Guid.Parse(managerIdClaim)
            };

            _context.ExportHistories.Add(exportHistory);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Export created successfully",
                exportId = exportHistory.ExportID,
                totalItems = approvedTasks.Count
            });
        }

        /// <summary>
        /// [Role: Manager] Lấy lịch sử các lần export của dự án.
        /// </summary>
        /// <param name="projectId">ID của dự án (Guid)</param>
        /// <response code="200">Trả về danh sách lịch sử export, sắp xếp mới nhất trước.</response>
        [Authorize(Roles = "Manager")]
        [HttpGet("{projectId}/export-histories")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async System.Threading.Tasks.Task<IActionResult> GetExportHistories(Guid projectId)
        {
            var histories = await _context.ExportHistories
                .Where(x => x.ProjectID == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.ExportID,
                    x.Format,
                    x.ItemCount,
                    x.CreatedAt,
                    x.ManagerID
                })
                .ToListAsync();

            return Ok(histories);
        }

        /// <summary>
        /// [Role: Tất cả] Lấy lịch sử thay đổi điểm tín nhiệm của một user.
        /// </summary>
        /// <remarks>
        /// Trả về danh sách các lần cộng/trừ điểm, lý do và task liên quan.
        /// Sắp xếp theo thời gian mới nhất trước.
        /// </remarks>
        /// <param name="userId">ID của user (Guid)</param>
        /// <response code="200">Trả về danh sách reputation logs.</response>
        /// <response code="404">User không tồn tại.</response>
        [Authorize]
        [HttpGet("users/{userId}/reputation-logs")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        [ProducesResponseType(404)]
        public async System.Threading.Tasks.Task<IActionResult> GetReputationLogs(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var logs = await _context.ReputationLogs
                .Where(x => x.UserID == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.ReputationLogID,
                    x.ScoreChange,
                    x.Reason,
                    x.CreatedAt,
                    x.TaskID
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}