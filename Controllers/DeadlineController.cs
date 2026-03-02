using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;

namespace SWP_BE.Controllers
{
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
        //Update deadline
        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("projects/{id}/deadline")]
        public async Task<IActionResult> UpdateProjectDeadline(Guid id, [FromBody] DateTime newDeadline)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound("Project not found");

            project.Deadline = newDeadline;
            await _context.SaveChangesAsync();

            return Ok("Project deadline updated successfully");
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("tasks/{id}/deadline")]
        public async Task<IActionResult> UpdateTaskDeadline(Guid id, [FromBody] DateTime newDeadline)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound("Task not found");

            task.Deadline = newDeadline;
            await _context.SaveChangesAsync();

            return Ok("Task deadline updated successfully");
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("projects/{id}/overdue-tasks")]
        public async Task<IActionResult> GetOverdueTasks(Guid id)
        {
            var now = DateTime.Now;

            var overdueTasks = await _context.Tasks
                .Where(t => t.ProjectID == id
                            && t.Deadline < now
                            && t.Status != "Approved")
                .ToListAsync();

            return Ok(overdueTasks);
        }


    }
}
