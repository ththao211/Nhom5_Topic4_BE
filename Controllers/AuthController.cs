using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.DTOs.Login;
using SWP_BE.Models;
using SWP_BE.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static SWP_BE.Models.User;

namespace SWP_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        private readonly List<string> DefaultPasswords = new()
        {
             "111111",
             "000000",
             "222222",
             "123456",
             "12345"
        };

        public static class PasswordResetStore
        {
            public static Dictionary<string, (Guid userId, string otp, DateTime expire)>
                ResetTokens = new();
        }

        /// <summary>
        /// Đăng nhập vào hệ thống
        /// </summary>
        /// <param name="dto">Thông tin tài khoản và mật khẩu</param>
        /// <response code="200">Đăng nhập thành công (trả về Token) hoặc yêu cầu đổi mật khẩu lần đầu</response>
        /// <response code="401">Tài khoản/mật khẩu không chính xác hoặc tài khoản bị vô hiệu hóa</response>
        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.Username);
            if (user == null || !VerifyPassword(dto.Password, user.Password))
            {
                return Unauthorized(ApiResponse<object>.Fail("Tài khoản hoặc mật khẩu không chính xác"));
            }
            if (!user.IsActive)
            {
                return Unauthorized(ApiResponse<object>.Fail("Tài khoản đã bị vô hiệu hóa"));
            }

            // LOGIC TỪ FILE 1: Kiểm tra mật khẩu mặc định lần đầu đăng nhập

            bool isDefaultPassword = DefaultPasswords
            .Any(p => BCrypt.Net.BCrypt.Verify(p, user.Password));
            var token = GenerateJwtToken(user);
            string roleName = GetRoleName(user.Role);
            if (isDefaultPassword)
            {
                return Ok(new
                {
                    Token = token,
                    requirePasswordChange = true,
                    message = "You must change password before using system"
                });
            }

            var responseData = new LoginResponseDTO
            {
                Token = token,
                User = new UserInfoDTO
                {
                    UserId = user.UserID,
                    FullName = user.FullName,
                    RoleName = roleName
                }
            };

            return Ok(ApiResponse<LoginResponseDTO>.Ok(responseData, "Đăng nhập thành công"));
        }

        // --- CÁC API THÊM VÀO TỪ FILE 1 ---

        /// <summary>
        /// Đổi mật khẩu cho lần đăng nhập đầu tiên
        /// </summary>
        /// <param name="request">Chứa mật khẩu cũ và mật khẩu mới</param>
        /// <response code="200">Đổi mật khẩu thành công</response>
        /// <response code="400">Mật khẩu cũ không chính xác</response>
        /// <response code="401">Không có quyền truy cập (thiếu token)</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        [Authorize]
        [HttpPost("change-password-first-login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
            {
                return BadRequest("Old password incorrect");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            // Lưu ý: Đáng lẽ NewPassword nên được Hash lại thay vì lưu plain text. 
            // Bạn có thể cân nhắc sửa thành: user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            await _context.SaveChangesAsync();

            return Ok("Password changed successfully");
        }

        /// <summary>
        /// Yêu cầu đặt lại mật khẩu (Quên mật khẩu)
        /// </summary>
        /// <param name="request">Email của người dùng cần khôi phục</param>
        /// <response code="200">Tạo mã khôi phục (token) thành công</response>
        /// <response code="404">Không tìm thấy email trong hệ thống</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                return NotFound("User not found");

            var otp = new Random().Next(100000, 999999).ToString();

            PasswordResetStore.ResetTokens[user.Email] =
            (
                user.UserID,
                otp,
                DateTime.UtcNow.AddMinutes(10)
            );

            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                user.FullName,
                otp
            );

            return Ok("OTP sent to email");
        }

        /// <summary>
        /// Đặt lại mật khẩu mới bằng Token khôi phục
        /// </summary>
        /// <param name="request">Token khôi phục và mật khẩu mới</param>
        /// <response code="200">Đặt lại mật khẩu thành công</response>
        /// <response code="400">Token không hợp lệ hoặc đã hết hạn</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequests request)
        {
            if (!PasswordResetStore.ResetTokens.ContainsKey(request.Email))
                return BadRequest("OTP not found");

            var data = PasswordResetStore.ResetTokens[request.Email];

            // kiểm tra hết hạn
            if (data.expire < DateTime.UtcNow)
                return BadRequest("OTP expired");

            // kiểm tra OTP
            if (data.otp != request.Otp)
                return BadRequest("Invalid OTP");

            var user = await _context.Users.FindAsync(data.userId);

            if (user == null)
                return NotFound();

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            PasswordResetStore.ResetTokens.Remove(request.Email);

            await _context.SaveChangesAsync();

            return Ok("Password reset successfully");
        }

        // --- HẾT PHẦN THÊM TỪ FILE 1 ---

        /// <summary>
        /// Lấy thông tin cá nhân của người dùng đang đăng nhập
        /// </summary>
        /// <response code="200">Thông tin chi tiết của người dùng</response>
        /// <response code="401">Không có quyền truy cập hoặc token không hợp lệ</response>
        /// <response code="404">Không tìm thấy thông tin người dùng</response>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMe()
        {
            // FIX: Hỗ trợ tìm cả Claim chuẩn của .NET và Claim "sub" của Frontend
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

            if (userIdClaim == null)
                return Unauthorized(new { message = "Không tìm thấy thông tin định danh trong Token." });

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Unauthorized(new { message = "Định dạng ID không hợp lệ." });

            var user = await _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => new UserProfileDto
                {
                    UserID = u.UserID,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    Score = u.Score,
                    CurrentTaskCount = u.CurrentTaskCount,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }

        private string GetRoleName(UserRole role) => ((int)role) switch
        {
            1 => "Admin",
            2 => "Manager",
            3 => "Annotator",
            4 => "Reviewer",
            _ => "Unknown"
        };

        private string GenerateJwtToken(User user)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddHours(3);
            var roleName = GetRoleName(user.Role);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("role", roleName),
                new Claim("full_name", user.FullName ?? ""),
                new Claim("username", user.UserName ?? ""),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"] ?? "authenticated",
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
            }
            catch
            {
                return inputPassword == storedPassword;
            }
        }
    }
}