using Microsoft.AspNetCore.Mvc;
using SWP_BE.Data;
using SWP_BE.Models;
using Microsoft.EntityFrameworkCore;


namespace SWP_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScoreController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetScores(
            [FromQuery] string? role,
            [FromQuery] Guid? userId)
        {
            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.Role == SWP_BE.Models.User.UserRole.Annotator
                         || u.Role == SWP_BE.Models.User.UserRole.Reviewer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                if (Enum.TryParse<SWP_BE.Models.User.UserRole>(role, true, out var parsedRole))
                {
                    query = query.Where(u => u.Role == parsedRole);
                }
            }

            if (userId.HasValue)
            {
                query = query.Where(u => u.UserID == userId.Value);
            }

            var result = await query
                .Select(u => new
                {
                    u.UserID,
                    u.UserName,
                    u.FullName,
                    Role = u.Role.ToString(),
                    CurrentScore = u.Score,
                    TotalScoreChange = _context.ReputationLogs
                        .Where(r => r.UserID == u.UserID)
                        .Sum(r => (int?)r.ScoreChange) ?? 0,

                    TaskCount = u.Role == SWP_BE.Models.User.UserRole.Annotator
                        ? u.AnnotatorTasks.Count()
                        : u.ReviewerTasks.Count()
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}