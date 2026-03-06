using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs.AdminDTO;
using SWP_BE.Models;
using static SWP_BE.Models.User;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SWP_BE.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) { _context = context; }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(x => x.UserName == dto.Username))
                return BadRequest("Username already exists");

            var user = new User
            {
                UserID = Guid.NewGuid(),
                UserName = dto.Username.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                Expertise = dto.Expertise?.Trim() ?? string.Empty,
                Role = (UserRole)dto.Role,
                IsActive = true,
                Score = 100
            };

            _context.Users.Add(user);
            await LogActivity("Create User", user.UserID);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User created successfully",
                userId = user.UserID
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("toggle-status/{id}")]
        public async Task<IActionResult> ToggleStatus(Guid id)
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

        [Authorize(Roles = "Admin")]
        [HttpPost("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(Guid id, [FromBody] UpdateRoleDTO dto)
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
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _context.Users.Where(u => u.IsActive).ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/api/admin/all-roles")]
        public IActionResult GetAllRoles()
        {
            var roles = Enum.GetValues(typeof(UserRole))
                            .Cast<UserRole>()
                            .Select(r => new { Id = (int)r, Name = r.ToString() });
            return Ok(roles);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("/api/admin/filter-by-roles")]
        public async Task<IActionResult> FilterUsers(int? role, bool? isActive, string? keyword)
        {
            var query = _context.Users.AsQueryable();

            if (role.HasValue)
            {
                if (role < 1 || role > 4) return BadRequest("Role phải từ 1-4");
                query = query.Where(u => (int)u.Role == role);
            }

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u =>
                    u.UserName.Contains(keyword) ||
                    u.FullName.Contains(keyword) ||
                    u.Email.Contains(keyword));
            }

            var result = await query
                .Select(u => new
                {
                    u.UserID,
                    u.UserName,
                    u.FullName,
                    u.Email,
                    Role = u.Role.ToString(),
                    u.Score,
                    u.CurrentTaskCount,
                    u.IsActive
                })
                .ToListAsync();

            return Ok(result);
        }

        // --- ĐÂY LÀ HÀM ĐÃ ĐƯỢC THÊM LOGIC BẢO MẬT ---
        [Authorize(Roles = "Admin")]
        [HttpPatch("assign-role/{id}")]
        public async Task<IActionResult> AssignRole(Guid id, [FromBody] UserRole newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found.");

            if (!Enum.IsDefined(typeof(UserRole), newRole))
                return BadRequest("Invalid role.");

            // Lấy ID của Admin đang thực hiện lệnh này
            var currentUserIdStr = User.FindFirst("id")?.Value
                                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? User.FindFirst("sub")?.Value;

            if (Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                // 1. Không cho tự hạ cấp chính mình
                if (user.UserID == currentUserId && newRole != UserRole.Admin)
                    return BadRequest("You cannot downgrade your own Admin role.");

                // 2. Không cho hạ cấp một Admin khác
                if (user.Role == UserRole.Admin && user.UserID != currentUserId && newRole != UserRole.Admin)
                    return BadRequest("Cannot downgrade another Admin.");
            }

            user.Role = newRole;
            await LogActivity("Assign Role", user.UserID);
            await _context.SaveChangesAsync();
            return Ok("Role assigned successfully.");
        }

        [Authorize]
        [HttpPut("update-user-info")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserId = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            await LogActivity("Deactivate User", user.UserID);
            await _context.SaveChangesAsync();
            return Ok("Deactivated");
        }

        private System.Threading.Tasks.Task LogActivity(string action, Guid targetUserId)
        {
            var currentUserIdStr = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var log = new ActivityLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                TargetUserId = targetUserId,
                PerformedBy = Guid.TryParse(currentUserIdStr, out var parsedId) ? parsedId : (Guid?)null,
                CreatedAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/api/admin/system-configs")]
        public async Task<IActionResult> GetConfigs()
        {
            var configs = await _context.SystemConfigs
                             .Include(x => x.Admin)
                             .OrderByDescending(x => x.UpdatedAt)
                             .ToListAsync();
            return Ok(configs);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("/api/admin/update-configs/{id}")]
        public async Task<IActionResult> UpdateConfig(int id, [FromBody] SystemConfig dto)
        {
            var config = await _context.SystemConfigs.FindAsync(id);
            if (config == null) return NotFound("Config not found.");

            var currentAdminId = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            config.Value = dto.Value;
            config.MaxProjectStorageMB = dto.MaxProjectStorageMB;
            config.AllowedFileTypes = dto.AllowedFileTypes;
            config.UpdatedAt = DateTime.UtcNow;

            if (Guid.TryParse(currentAdminId, out var adminGuid))
            {
                config.AdminID = adminGuid;
                await LogActivity("Update System Config", adminGuid);
            }

            await _context.SaveChangesAsync();
            return Ok("System configuration updated successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/api/admin/system-logs/filter")]
        public async Task<IActionResult> FilterLogs(Guid? userId, string? action, DateTime? fromDate, DateTime? toDate)
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