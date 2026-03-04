using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.DTOs.Login;
using SWP_BE.Models;
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
            var claims = new[]
            {
                new Claim("id", user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, GetRoleName(user.Role))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
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