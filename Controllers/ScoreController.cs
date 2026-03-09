using Microsoft.AspNetCore.Mvc;
using SWP_BE.Data;
using SWP_BE.Models;
using Microsoft.EntityFrameworkCore;
// Thêm thư viện này để dùng các StatusCodes cho Swagger
using Microsoft.AspNetCore.Http; 

namespace SWP_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Score")] 
    public class ScoreController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScoreController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách điểm số và thống kê của Annotator hoặc Reviewer.
        /// </summary>
        /// <remarks>
        /// Trả về danh sách người dùng kèm theo điểm số hiện tại, tổng điểm thay đổi và số lượng task đã làm.
        /// Có thể lọc theo vai trò (Role) hoặc ID người dùng (UserID).
        /// </remarks>
        /// <param name="role">Vai trò cần lọc (vd: "Annotator", "Reviewer"). Bỏ trống để lấy tất cả.</param>
        /// <param name="userId">ID cụ thể của người dùng cần tìm. Bỏ trống để lấy tất cả.</param>
        /// <returns>Danh sách thống kê điểm số của người dùng.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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