using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWP_BE.Services
{
    public interface IScoreService
    {
        Task<List<UserScoreResponseDto>> GetAllScores();
        Task<List<UserScoreResponseDto>> GetByRole(User.UserRole role);
        Task<UserScoreResponseDto?> GetByUserId(Guid userId);
    }

    public class ScoreService : IScoreService
    {
        private readonly AppDbContext _context;

        public ScoreService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserScoreResponseDto>> GetAllScores()
        {
            return await _context.Users
                .AsNoTracking() // Tối ưu hiệu năng
                .Where(u => u.Role == User.UserRole.Annotator
                         || u.Role == User.UserRole.Reviewer)
                .Select(u => new UserScoreResponseDto
                {
                    UserId = u.UserID,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Role = u.Role,
                    CurrentScore = u.Score,

                    // FIX CS0229: Truy vấn trực tiếp từ DbSet ReputationLogs
                    TotalScoreChange = _context.ReputationLogs
                        .Where(r => r.UserID == u.UserID)
                        .Sum(r => (int?)r.ScoreChange) ?? 0,

                    TaskCount = u.Role == User.UserRole.Annotator
                        ? u.AnnotatorTasks.Count()
                        : u.ReviewerTasks.Count()
                })
                .ToListAsync();
        }

        public async Task<List<UserScoreResponseDto>> GetByRole(User.UserRole role)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == role)
                .Select(u => new UserScoreResponseDto
                {
                    UserId = u.UserID,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Role = u.Role,
                    CurrentScore = u.Score,

                    // FIX CS0229: Tránh sử dụng navigation property bị lỗi
                    TotalScoreChange = _context.ReputationLogs
                        .Where(r => r.UserID == u.UserID)
                        .Sum(r => (int?)r.ScoreChange) ?? 0,

                    TaskCount = role == User.UserRole.Annotator
                        ? u.AnnotatorTasks.Count()
                        : u.ReviewerTasks.Count()
                })
                .ToListAsync();
        }

        public async Task<UserScoreResponseDto?> GetByUserId(Guid userId)
        {
            // Truy vấn User mà không dùng Include cho các thuộc tính bị nhập nhằng
            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.UserID == userId);

            return await query
                .Select(u => new UserScoreResponseDto
                {
                    UserId = u.UserID,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Role = u.Role,
                    CurrentScore = u.Score,

                    // Tính toán trực tiếp để đảm bảo chính xác
                    TotalScoreChange = _context.ReputationLogs
                        .Where(r => r.UserID == u.UserID)
                        .Sum(r => (int?)r.ScoreChange) ?? 0,

                    TaskCount = u.Role == User.UserRole.Annotator
                        ? u.AnnotatorTasks.Count()
                        : u.ReviewerTasks.Count()
                })
                .FirstOrDefaultAsync();
        }
    }
}