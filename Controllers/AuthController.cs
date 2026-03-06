using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.DTOs.Login;
using SWP_BE.Models;
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

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
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

            var token = GenerateJwtToken(user);
            string roleName = GetRoleName(user.Role);

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

        [Authorize]
        [HttpGet("me")]
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