using SWP_BE.Models;
using SWP_BE.Repositories;

namespace SWP_BE.Services
{
    public class ReputationService
    {
        private readonly IReputationRepository _repo;

        public ReputationService(IReputationRepository repo) => _repo = repo;

        /// <summary>
        /// Xử lý tính điểm khi Task kết thúc (Approved hoặc Fail)
        /// </summary>
        public async System.Threading.Tasks.Task HandleTaskCompletionAsync(Guid userId, Models.Task task)
        {
            var user = await _repo.GetUserForUpdateAsync(userId);
            if (user == null) return;

            var rules = (await _repo.GetAllActiveRulesAsync()).ToDictionary(r => r.RuleName, r => r);
            int scoreDelta = 0;
            string reason = "";
            int? appliedRuleId = null;

            // --- LOGIC TÍNH TOÁN ĐIỂM BIẾN ĐỘNG (scoreDelta) ---
            if (task.Status == Models.Task.TaskStatus.Approved)
            {
                switch (task.RejectCount)
                {
                    case 0: // Perfect
                        scoreDelta = rules["Reward_Perfect"].Value; // +20
                        reason = "Perfect: Hoàn thành ngay từ đầu";
                        appliedRuleId = rules["Reward_Perfect"].RuleID;
                        break;
                    case 1: // Sau sửa lần 1
                        scoreDelta = 0;
                        reason = "Approve sau sửa lần 1";
                        if (task.RateComplete > 95)
                        {
                            scoreDelta = 2;
                            reason += " (+2đ Bonus HighRate)";
                            appliedRuleId = rules["Bonus_HighRate"].RuleID;
                        }
                        break;
                    case 2: // Sau sửa lần 2
                        scoreDelta = rules["Penalty_Reject_2"].Value; // -5
                        reason = "Approve sau sửa lần 2";
                        appliedRuleId = rules["Penalty_Reject_2"].RuleID;
                        if (task.RateComplete > 95)
                        {
                            scoreDelta += 2;
                            reason += " (+2đ Bonus HighRate)";
                        }
                        break;
                    case 3: // Sau sửa lần 3
                        scoreDelta = rules["Penalty_Reject_3"].Value; // -10
                        reason = "Approve sau sửa lần 3";
                        appliedRuleId = rules["Penalty_Reject_3"].RuleID;
                        break;
                }
            }
            else if (task.Status == Models.Task.TaskStatus.Fail)
            {
                scoreDelta = rules["Penalty_Task_Fail"].Value; // -20
                reason = "Task bị Fail (Reject lần 4)";
                appliedRuleId = rules["Penalty_Task_Fail"].RuleID;
            }

            // --- LOGIC KIỂM TRA NGƯỠNG 0 - 100 ---
            int oldScore = user.Score;
            // Tính toán điểm mới và ép vào khoảng [0, 100]
            int newScore = oldScore + scoreDelta;

            if (newScore > 100) newScore = 100; // Chặn trên 100
            if (newScore < 0) newScore = 0;     // Chặn dưới 0

            user.Score = newScore;

            int actualChange = newScore - oldScore;

            // --- LƯU LOG VỚI SỐ ĐIỂM THỰC TẾ ---
            var log = new ReputationLog
            {
                UserID = userId,
                OldScore = oldScore,
                NewScore = newScore,
                ScoreChange = actualChange,
                Reason = reason,
                TaskID = task.TaskID,
                RuleID = appliedRuleId,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddLogAsync(log);

            // --- CHECK SA THẢI ---
            await CheckUserStatus(user, rules);

            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Logic Manager check trước khi Assign Task
        /// </summary>
        public async Task<(bool CanAssign, string Message)> CanManagerAssignTask(Guid annotatorId)
        {
            var user = await _repo.GetUserForUpdateAsync(annotatorId);
            if (user == null || !user.IsActive) return (false, "Annotator không tồn tại hoặc đã bị khóa.");

            var rules = (await _repo.GetAllActiveRulesAsync()).ToDictionary(r => r.RuleName, r => r.Value);

            int limit = 0;
            if (user.Score >= rules["High_Threshold"]) limit = rules["Max_Task_High"];
            else if (user.Score >= rules["Low_Threshold"]) limit = rules["Max_Task_Normal"];
            else limit = rules["Max_Task_Warning"];

            if (user.CurrentTaskCount >= limit)
                return (false, $"Quá giới hạn: Người này chỉ được làm tối đa {limit} task với mức điểm {user.Score}.");

            return (true, "Hợp lệ.");
        }

        private async System.Threading.Tasks.Task CheckUserStatus(User user, Dictionary<string, ReputationRule> rules)
        {
            // 1. Điểm về 0 -> Nghỉ việc
            if (user.Score <= 0) user.IsActive = false;

            // 2. Check 3 Task Fail liên tiếp
            int streakLimit = rules["Max_Consecutive_Fails"].Value;
            var latestLogs = await _repo.GetLatestFailLogsAsync(user.UserID, streakLimit);

            // Nếu 3 log gần nhất đều là Penalty_Task_Fail
            if (latestLogs.Count == streakLimit && latestLogs.All(l => l.RuleID == rules["Penalty_Task_Fail"].RuleID))
            {
                user.IsActive = false;
            }


        }

        // Trong ReputationService.cs

        public async Task<IEnumerable<ReputationRuleDto>> GetAllRulesForAdminAsync()
        {
            var rules = await _repo.GetAllRulesAsync();
            return rules.Select(r => new ReputationRuleDto
            {
                RuleID = r.RuleID,
                RuleName = r.RuleName,
                Value = r.Value,
                Category = r.Category,
                Description = r.Description,
                IsActive = r.IsActive,
                UpdatedAt = r.UpdatedAt
            });
        }

        public async Task<(bool Success, string Message)> UpdateRuleAsync(int ruleId, UpdateRuleDto dto)
        {
            var rule = await _repo.GetRuleByIdAsync(ruleId);
            if (rule == null)
                return (false, "Không tìm thấy cấu hình luật này.");

            // Cập nhật các thông số
            rule.Value = dto.Value;
            rule.Description = dto.Description;
            rule.IsActive = dto.IsActive;
            rule.UpdatedAt = DateTime.Now;

            await _repo.SaveChangesAsync();

            return (true, $"Đã cập nhật thành công luật: {rule.RuleName}");
        }
    }
}