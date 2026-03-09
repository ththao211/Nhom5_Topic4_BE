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
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

namespace SWP_BE.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) { _context = context; }

        /// <summary>
        /// Tạo mới một người dùng (Admin)
        /// </summary>
        /// <param name="dto">Thông tin người dùng cần tạo</param>
        /// <response code="200">Tạo người dùng thành công, trả về userId</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc Username đã tồn tại</response>
        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
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

        /// <summary>
        /// Bật/Tắt trạng thái hoạt động của người dùng
        /// </summary>
        /// <param name="id">ID của người dùng cần thay đổi trạng thái</param>
        /// <response code="200">Cập nhật trạng thái thành công</response>
        /// <response code="400">Lỗi khi thao tác (VD: tự vô hiệu hóa chính mình)</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        [Authorize(Roles = "Admin")]
        [HttpPatch("toggle-status/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Đặt lại mật khẩu cho người dùng
        /// </summary>
        /// <param name="id">ID của người dùng</param>
        /// <param name="dto">Chứa mật khẩu mới</param>
        /// <response code="200">Đặt lại mật khẩu thành công</response>
        /// <response code="400">Mật khẩu mới không được để trống</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        [Authorize(Roles = "Admin")]
        [HttpPost("reset-password/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Lấy danh sách tất cả người dùng đang hoạt động
        /// </summary>
        /// <response code="200">Danh sách người dùng đang hoạt động</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("all-users")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _context.Users.Where(u => u.IsActive).ToListAsync());
        }

        /// <summary>
        /// Lấy danh sách tất cả các Role trong hệ thống
        /// </summary>
        /// <response code="200">Danh sách các Role</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("/api/admin/all-roles")]
        [ProducesResponseType(200)]
        public IActionResult GetAllRoles()
        {
            var roles = Enum.GetValues(typeof(UserRole))
                            .Cast<UserRole>()
                            .Select(r => new { Id = (int)r, Name = r.ToString() });
            return Ok(roles);
        }

        /// <summary>
        /// Lọc danh sách người dùng theo Role, trạng thái và từ khóa
        /// </summary>
        /// <param name="role">Role ID (1-4)</param>
        /// <param name="isActive">Trạng thái hoạt động</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo Username, FullName hoặc Email</param>
        /// <response code="200">Danh sách người dùng sau khi lọc</response>
        /// <response code="400">Role không hợp lệ</response>
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("/api/admin/filter-by-roles")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
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

        /// <summary>
        /// Phân quyền (gán Role) cho người dùng
        /// </summary>
        /// <param name="id">ID người dùng</param>
        /// <param name="newRole">Role mới cần gán</param>
        /// <response code="200">Cập nhật quyền thành công</response>
        /// <response code="400">Role không hợp lệ hoặc vi phạm logic phân quyền</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        [Authorize(Roles = "Admin")]
        [HttpPatch("assign-role/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignRole(Guid id, [FromBody] UserRole newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found.");

            if (!Enum.IsDefined(typeof(UserRole), newRole))
                return BadRequest("Invalid role.");

            var currentUserIdStr = User.FindFirst("id")?.Value
                                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? User.FindFirst("sub")?.Value;

            if (Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                if (user.UserID == currentUserId && newRole != UserRole.Admin)
                    return BadRequest("You cannot downgrade your own Admin role.");

                if (user.Role == UserRole.Admin && user.UserID != currentUserId && newRole != UserRole.Admin)
                    return BadRequest("Cannot downgrade another Admin.");
            }

            user.Role = newRole;
            await LogActivity("Assign Role", user.UserID);
            await _context.SaveChangesAsync();
            return Ok("Role assigned successfully.");
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân của người dùng
        /// </summary>
        /// <param name="id">ID người dùng cần cập nhật</param>
        /// <param name="dto">Thông tin mới</param>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="403">Không có quyền truy cập</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        [Authorize]
        [HttpPut("update-user-info")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Vô hiệu hóa người dùng (Soft Delete)
        /// </summary>
        /// <param name="id">ID người dùng</param>
        /// <response code="200">Vô hiệu hóa thành công</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-user/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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
            // Kết hợp logic lấy Claim của File 1, đảm bảo tương thích tốt nhất
            var currentUserIdStr = User.FindFirst("id")?.Value
                                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

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

        /// <summary>
        /// Lấy thông tin cấu hình hệ thống
        /// </summary>
        /// <response code="200">Trả về danh sách cấu hình hệ thống</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("/api/admin/system-configs")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetConfigs()
        {
            var configs = await _context.SystemConfigs
                             .Include(x => x.Admin)
                             .OrderByDescending(x => x.UpdatedAt)
                             .ToListAsync();
            return Ok(configs);
        }

        /// <summary>
        /// Cập nhật cấu hình hệ thống
        /// </summary>
        /// <param name="id">ID cấu hình</param>
        /// <param name="dto">Dữ liệu cấu hình mới</param>
        /// <response code="200">Cập nhật cấu hình thành công</response>
        /// <response code="404">Không tìm thấy cấu hình</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("/api/admin/update-configs/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Lọc danh sách System Logs
        /// </summary>
        /// <param name="userId">ID người thực hiện</param>
        /// <param name="action">Hành động (chứa từ khóa)</param>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <response code="200">Danh sách Logs sau khi lọc</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("/api/admin/system-logs/filter")]
        [ProducesResponseType(200)]
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