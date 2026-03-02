using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.Models;
using System.Security.Claims;

namespace SWP_BE.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectStatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectStatsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{projectId}/statistics")]
        public async Task<IActionResult> GetProjectStatistics(Guid projectId)
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

        [HttpGet("{taskId}/user-performance")]
        public async Task<IActionResult> GetUserPerformance(Guid taskId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.ProjectID == taskId)
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
                    .Average(x => (x.CompletedAt - x.CreatedAt).TotalHours)
                });
            return Ok(performance);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("{projectId}/exports")]
        public async Task<IActionResult> ExportProject(Guid projectId, [FromBody] ExportRequest request)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound("Project not found");

            var approvedTasks = await _context.Tasks
                .Where(t => t.ProjectID == projectId && t.Status == "Approved")
                .ToListAsync();

            if (!approvedTasks.Any())
                return BadRequest("No approved tasks to export");

            var managerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

        [Authorize(Roles = "Manager")]
        [HttpGet("{projectId}/export-histories")]
        public async Task<IActionResult> GetExportHistories(Guid projectId)
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

        [Authorize]
        [HttpGet("users/{userId}/reputation-logs")]
        public async Task<IActionResult> GetReputationLogs(Guid userId)
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

