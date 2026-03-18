using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;

namespace SWP_BE.Services
{
    // 1. INTERFACE
    public interface IStatisticsService
    {
        System.Threading.Tasks.Task UpdateAnnotatorStatsAsync(Guid userId, double completionHours, int rejectCount, bool isFinalFail = false);
        System.Threading.Tasks.Task UpdateReviewerStatsAsync(Guid userId, double reviewHours);
        // Hai hàm này sẽ build logic sắp xếp ở bước tiếp theo
        Task<object> GetAnnotatorSuggestionsAsync(Guid projectId);
        Task<object> GetReviewerSuggestionsAsync(Guid projectId);
    }

    // 2. IMPLEMENTATION
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task UpdateAnnotatorStatsAsync(Guid userId, double completionHours, int rejectCount, bool isFinalFail = false)
        {
            var user = await _context.Users.FindAsync(userId);
            var stats = await _context.AnnotatorStats.FirstOrDefaultAsync(s => s.UserID == userId);

            if (stats == null)
            {
                stats = new AnnotatorStat { UserID = userId };
                _context.AnnotatorStats.Add(stats);
            }

            // Luôn giảm số Task đang giữ khi kết thúc vòng đời 1 Task (Approve hoặc Fail)
            if (user != null && user.CurrentTaskCount > 0)
            {
                user.CurrentTaskCount--;
            }

            // Nếu Task thành công (Approved) mới tính các chỉ số hiệu suất
            if (!isFinalFail)
            {
                stats.TotalCompletedTasks += 1;
                stats.TotalWorkingHours += completionHours;

                // Tránh chia cho 0 mặc dù đã check logic
                if (stats.TotalCompletedTasks > 0)
                {
                    stats.AvgCompletionHours = stats.TotalWorkingHours / stats.TotalCompletedTasks;
                }

                // Tiêu chí 3: Tỷ lệ duyệt lần đầu
                if (rejectCount == 0)
                {
                    stats.FirstTryApprovedTasks += 1;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateReviewerStatsAsync(Guid userId, double reviewHours)
        {
            var stats = await _context.ReviewerStats.FirstOrDefaultAsync(s => s.UserID == userId);

            if (stats == null)
            {
                stats = new ReviewerStat { UserID = userId };
                _context.ReviewerStats.Add(stats);
            }

            stats.TotalReviewedTasks += 1;
            stats.TotalReviewHours += reviewHours;

            if (stats.TotalReviewedTasks > 0)
            {
                stats.AvgReviewHours = stats.TotalReviewHours / stats.TotalReviewedTasks;
            }

            await _context.SaveChangesAsync();
        }

        // Tạm thời để trống để chờ "Okay" từ bạn
        public async Task<object> GetAnnotatorSuggestionsAsync(Guid projectId) => throw new NotImplementedException();
        public async Task<object> GetReviewerSuggestionsAsync(Guid projectId) => throw new NotImplementedException();
    }
}