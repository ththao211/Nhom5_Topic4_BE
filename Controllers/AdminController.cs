using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs.AdminDTO;
using SWP_BE.Models;
using static SWP_BE.Models.User;
using Task = System.Threading.Tasks.Task;

namespace SWP_BE.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) { _context = context; }

        [HttpPost("create-user")]
        public async System.Threading.Tasks.Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (dto.Role < 1 || dto.Role > 4)
                return BadRequest("Role không hợp lệ! (1:Admin, 2:Manager, 3:Annotator, 4:Reviewer)");

            if (await _context.Users.AnyAsync(x => x.UserName == dto.Username))
                return BadRequest("Username already exists");

            var user = new User
            {
                UserID = Guid.NewGuid(),
                UserName = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Email = dto.Email,
                Expertise = dto.Expertise,
                Role = (UserRole)dto.Role,
                IsActive = true,
                Score = 100
            };

            _context.Users.Add(user);
            await LogActivity("Create User", user.UserID);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created successfully", userId = user.UserID });
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("toggle-status/{id}")]
        public async System.Threading.Tasks.Task<IActionResult> ToggleStatus(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            var currentUserName = User.Identity?.Name;
            if (user.UserName == currentUserName)
                return BadRequest("You cannot deactivate yourself.");

            user.IsActive = !user.IsActive;

            await LogActivity("Toggle Status", user.UserID);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User status updated", isActive = user.IsActive });
        }

        [HttpPost("reset-password/{id}")]
        public async System.Threading.Tasks.Task<IActionResult> ResetPassword(Guid id, [FromBody] UpdateRoleDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found.");
            if (string.IsNullOrEmpty(dto.NewPassword)) return BadRequest("Password cannot be empty.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await LogActivity("Reset Password", user.UserID);
            await _context.SaveChangesAsync();
            return Ok("Password reset successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all-users")]
        public async System.Threading.Tasks.Task<IActionResult> GetAllUsers()
        {
            return Ok(await _context.Users.Where(u => u.IsActive).ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/api/admin/roles")]
        public IActionResult GetAllRoles()
        {
            var roles = Enum.GetValues(typeof(UserRole))
                            .Cast<UserRole>()
                            .Select(r => new { Id = (int)r, Name = r.ToString() });
            return Ok(roles);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("assign-role/{id}")]
        public async System.Threading.Tasks.Task<IActionResult> AssignRole(Guid id, [FromBody] UserRole newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found.");
            if (!Enum.IsDefined(typeof(UserRole), newRole)) return BadRequest("Invalid role.");

            user.Role = newRole;
            await LogActivity("Assign Role", user.UserID);
            await _context.SaveChangesAsync();
            return Ok("Role assigned successfully.");
        }

        [Authorize]
        [HttpPut("update-user-info")]
        public async System.Threading.Tasks.Task<IActionResult> UpdateUser(Guid id, UpdateUserDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserId = User.FindFirst("id")?.Value;
            var currentRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (user.UserID.ToString() != currentUserId && currentRole != "Admin")
                return Forbid("Access denied.");

            if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            if (dto.Role != 0 && currentRole == "Admin")
            {
                if (dto.Role < 1 || dto.Role > 4) return BadRequest("Role không hợp lệ!");
                user.Role = (UserRole)dto.Role;
            }

            await LogActivity("Update User", user.UserID);
            await _context.SaveChangesAsync();
            return Ok("Updated");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-user/{id}")]
        public async System.Threading.Tasks.Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            await LogActivity("Deactivate User", user.UserID);
            await _context.SaveChangesAsync();
            return Ok("Deactivated");
        }

        private async Task LogActivity(string action, Guid targetUserId)
        {
            var currentUserIdStr = User.FindFirst("id")?.Value;
            var log = new ActivityLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                TargetUserId = targetUserId,
                PerformedBy = currentUserIdStr != null ? Guid.Parse(currentUserIdStr) : null,
                CreatedAt = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/api/admin/system-configs")]
        public async System.Threading.Tasks.Task<IActionResult> GetConfigs()
        {
            var configs = await _context.SystemConfigs
                             .Include(x => x.Admin)
                             .OrderByDescending(x => x.UpdatedAt)
                             .ToListAsync();
            return Ok(configs);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("/api/admin/update-configs/{id}")]
        public async System.Threading.Tasks.Task<IActionResult> UpdateConfig(int id, [FromBody] SystemConfig dto)
        {
            var config = await _context.SystemConfigs.FindAsync(id);
            if (config == null) return NotFound("Config not found.");

            var currentAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            config.Value = dto.Value;
            config.MaxProjectStorageMB = dto.MaxProjectStorageMB;
            config.AllowedFileTypes = dto.AllowedFileTypes;
            config.UpdatedAt = DateTime.UtcNow;

            if (currentAdminId != null) config.AdminID = Guid.Parse(currentAdminId);

            if (currentAdminId != null)
                await LogActivity("Update System Config", Guid.Parse(currentAdminId));

            await _context.SaveChangesAsync();
            return Ok("System configuration updated successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/api/admin/system-logs/filter")]
        public async System.Threading.Tasks.Task<IActionResult> FilterLogs(Guid? userId, string? action, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.ActivityLogs.AsQueryable();
            if (userId.HasValue) query = query.Where(x => x.PerformedBy == userId.Value);
            if (!string.IsNullOrEmpty(action)) query = query.Where(x => x.Action.Contains(action));
            if (fromDate.HasValue) query = query.Where(x => x.CreatedAt >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(x => x.CreatedAt <= toDate.Value);

            var logs = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
            return Ok(logs);
        }
    }
}