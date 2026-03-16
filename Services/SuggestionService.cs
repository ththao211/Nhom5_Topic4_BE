using SWP_BE.DTOs;
using SWP_BE.Repositories;

namespace SWP_BE.Services
{
    public class SuggestionService
    {
        private readonly SuggestionRepository _repo;
        public SuggestionService(SuggestionRepository repo) => _repo = repo;

        // --- GỢI Ý ANNOTATOR (7 Tầng ưu tiên) ---
        public async Task<List<SuggestionDto>> GetAnnotatorSuggestions(Guid projectId)
        {
            var project = await _repo.GetProjectAsync(projectId);
            var users = await _repo.GetUsersByRoleAsync("Annotator");

            return users.Select(u => new {
                User = u,
                Stat = u.AnnotatorStat,
                IsMatch = u.Expertise == project?.ProjectType, // Tầng 1
                // Tầng 3: Tỷ lệ duyệt đầu
                Rate = u.AnnotatorStat?.TotalCompletedTasks > 0
                    ? (double)u.AnnotatorStat.FirstTryApprovedTasks / u.AnnotatorStat.TotalCompletedTasks
                    : 0
            })
            .OrderByDescending(x => x.IsMatch)                            // 1. Chuyên môn
            .ThenByDescending(x => x.User.Score)                // 2. Tín nhiệm
            .ThenByDescending(x => x.Rate)                                // 3. Tỷ lệ duyệt đầu
            .ThenBy(x => x.User.CurrentTaskCount)                         // 4. Số task đang giữ (Max 3)
            .ThenBy(x => x.Stat?.AvgCompletionHours ?? 999)               // 5. Thời gian TB
            .ThenByDescending(x => x.Stat?.TotalCompletedTasks ?? 0)      // 6. Kinh nghiệm
            .ThenByDescending(x => x.Stat?.CurrentPerfectStreak ?? 0)     // 7. Phong độ
            .Select(x => new SuggestionDto
            {
                UserId = x.User.UserID,
                UserName = x.User.UserName,
                Expertise = x.User.Expertise,
                ReputationScore = x.User.Score,
                CurrentTaskCount = x.User.CurrentTaskCount,
                FirstTryRate = Math.Round(x.Rate * 100, 1),
                Experience = x.Stat?.TotalCompletedTasks ?? 0,
                AvgHours = Math.Round(x.Stat?.AvgCompletionHours ?? 0, 1),
                PerfectStreak = x.Stat?.CurrentPerfectStreak ?? 0,
                SuggestionNote = x.IsMatch ? "Đúng chuyên môn" : "Khác chuyên môn"
            }).ToList();
        }

        // --- GỢI Ý REVIEWER (Loại bỏ Tầng 3 & 7) ---
        public async Task<List<SuggestionDto>> GetReviewerSuggestions(Guid projectId)
        {
            var project = await _repo.GetProjectAsync(projectId);
            var users = await _repo.GetUsersByRoleAsync("Reviewer");

            return users.Select(u => new {
                User = u,
                Stat = u.ReviewerStat,
                IsMatch = u.Expertise == project?.ProjectType // Tầng 1
            })
            .OrderByDescending(x => x.IsMatch)                            // 1. Chuyên môn
            .ThenByDescending(x => x.User.Score)                // 2. Tín nhiệm
            // Tầng 3 bị loại bỏ cho Reviewer
            .ThenBy(x => x.User.CurrentTaskCount)                         // 4. Số task đang giữ
            .ThenByDescending(x => x.Stat?.TotalReviewedTasks ?? 0)       // 5. Kinh nghiệm
            .ThenBy(x => x.Stat?.AvgReviewHours ?? 999)                   // 6. Thời gian review TB
            .Select(x => new SuggestionDto
            {
                UserId = x.User.UserID,
                UserName = x.User.UserName,
                Expertise = x.User.Expertise,
                ReputationScore = x.User.Score,
                CurrentTaskCount = x.User.CurrentTaskCount,
                Experience = x.Stat?.TotalReviewedTasks ?? 0,
                AvgHours = Math.Round(x.Stat?.AvgReviewHours ?? 0, 1),
                SuggestionNote = x.IsMatch ? "Reviewer chuyên nghiệp" : "Reviewer sẵn sàng"
            }).ToList();
        }
    }
}