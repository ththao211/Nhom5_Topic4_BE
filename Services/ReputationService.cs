using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using SWP_BE.Repositories;
using static UpdateRuleDto;

namespace SWP_BE.Services
{
    public class ReputationService
    {
        private readonly IReputationRepository _repo;
        private readonly AppDbContext _context;

        public ReputationService(IReputationRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async System.Threading.Tasks.Task HandleTaskCompletionAsync(Guid userId, Models.Task task)
        {
            var user = await _repo.GetUserForUpdateAsync(userId);
            if (user == null) return;

            var rules = (await _repo.GetAllActiveRulesAsync()).ToDictionary(r => r.RuleName, r => r);
            int scoreDelta = 0;
            string reason = "";
            int? appliedRuleId = null;

            if (task.Status == Models.Task.TaskStatus.Approved || task.Status == Models.Task.TaskStatus.Rejected)
            {
                switch (task.CurrentRound)
                {
                    case 1:
                        scoreDelta = rules["Reward_Perfect"].Value;
                        reason = "Perfect: Hoàn thành ngay từ đầu";
                        appliedRuleId = rules["Reward_Perfect"].RuleID;
                        break;
                    case 2:
                        scoreDelta = 0;
                        reason = "Approve sau sửa lần 1";
                        if (task.RateComplete > 95)
                        {
                            scoreDelta = 2;
                            reason += " (+2đ Bonus HighRate)";
                            appliedRuleId = rules["Bonus_HighRate"].RuleID;
                        }
                        break;
                    case 3:
                        scoreDelta = rules["Penalty_Reject_2"].Value;
                        reason = "Approve sau sửa lần 2";
                        appliedRuleId = rules["Penalty_Reject_2"].RuleID;
                        if (task.RateComplete > 95) scoreDelta += 2;
                        break;
                    case 4:
                        scoreDelta = rules["Penalty_Reject_3"].Value;
                        reason = "Approve sau sửa lần 3";
                        appliedRuleId = rules["Penalty_Reject_3"].RuleID;
                        break;
                }
            }
            else if (task.Status == Models.Task.TaskStatus.Fail)
            {
                scoreDelta = rules["Penalty_Task_Fail"].Value;
                reason = "Task bị Fail (Reject lần 4)";
                appliedRuleId = rules["Penalty_Task_Fail"].RuleID;
            }

            int oldScore = user.Score;
            user.Score = CalculateClampedScore(oldScore, scoreDelta);

            // GIẢI PHÓNG SLOT CHO CẢ ANNOTATOR VÀ REVIEWER
            if (task.Status == Models.Task.TaskStatus.Approved || task.Status == Models.Task.TaskStatus.Fail)
            {
                // Trừ cho Annotator (Người được truyền vào userId)
                if (user.CurrentTaskCount > 0) user.CurrentTaskCount -= 1;

                // Trừ cho Reviewer
                if (task.ReviewerID.HasValue)
                {
                    var reviewer = await _context.Users.FindAsync(task.ReviewerID.Value);
                    if (reviewer != null && reviewer.CurrentTaskCount > 0)
                    {
                        reviewer.CurrentTaskCount -= 1;
                    }
                }
            }

            var log = new ReputationLog
            {
                UserID = userId,
                OldScore = oldScore,
                NewScore = user.Score,
                ScoreChange = scoreDelta,
                Reason = reason,
                TaskID = task.TaskID,
                RuleID = appliedRuleId,
                CreatedAt = DateTime.UtcNow
            };

            var annoStat = await _context.AnnotatorStats.FirstOrDefaultAsync(s => s.UserID == userId);
            if (annoStat != null)
            {
                annoStat.TotalCompletedTasks++;
                annoStat.RejectDisputedTasksStreak = 0;
                if (task.Status == Models.Task.TaskStatus.Approved && task.CurrentRound == 1)
                {
                    annoStat.FirstTryApprovedTasks++;
                    annoStat.CurrentPerfectStreak++;
                }
                double workHours = (DateTime.UtcNow - task.CreatedAt).TotalHours;
                annoStat.TotalWorkingHours += workHours;
                annoStat.AvgCompletionHours = annoStat.TotalWorkingHours / annoStat.TotalCompletedTasks;
            }

            var revStat = await _context.ReviewerStats.FirstOrDefaultAsync(s => s.UserID == task.ReviewerID.Value);
            if (revStat != null && task.Status == Models.Task.TaskStatus.Approved)
            {
                revStat.TotalReviewedTasks++;
                double reviewDuration = (DateTime.UtcNow - task.CreatedAt).TotalHours;
                revStat.TotalReviewHours += reviewDuration;
                revStat.AvgReviewHours = revStat.TotalReviewHours / revStat.TotalReviewedTasks;
            }

            await _repo.AddLogAsync(log);
            await CheckUserStatus(user, rules);
            await _repo.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task HandleReviewerDisputeLossAsync(Guid reviewerId, Guid taskId)
        {
            var reviewer = await _repo.GetUserForUpdateAsync(reviewerId);
            if (reviewer == null) return;

            var rules = (await _repo.GetAllActiveRulesAsync()).ToDictionary(r => r.RuleName, r => r);

            int oldScore = reviewer.Score;
            int penalty = rules["Penalty_Reviewer_False_Check"].Value;
            reviewer.Score = CalculateClampedScore(oldScore, penalty);

            await _repo.AddLogAsync(new ReputationLog
            {
                UserID = reviewerId,
                OldScore = oldScore,
                NewScore = reviewer.Score,
                ScoreChange = penalty,
                Reason = "Reviewer bắt lỗi sai (Dispute Accepted by Manager)",
                TaskID = taskId,
                RuleID = rules["Penalty_Reviewer_False_Check"].RuleID,
                CreatedAt = DateTime.UtcNow
            });

            var revStat = await _context.ReviewerStats.FirstOrDefaultAsync(s => s.UserID == reviewerId);
            if (revStat != null)
            {
                revStat.DisputedTasksStreak++;
                revStat.CurrentPerfectRejectStreak = 0;
                int maxStreak = rules["Max_Disputed_Tasks_Streak"].Value;
                if (revStat.DisputedTasksStreak >= maxStreak) reviewer.IsActive = false;
            }

            if (reviewer.Score <= 0) reviewer.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task HandleAnnotatorDisputeLossAsync(Guid annotatorId, Guid taskId)
        {
            var user = await _repo.GetUserForUpdateAsync(annotatorId);
            if (user == null) return;

            var rules = (await _repo.GetAllActiveRulesAsync()).ToDictionary(r => r.RuleName, r => r);

            int oldScore = user.Score;
            int penalty = rules["Penalty_Annotator_Rejected_Dispute"].Value;
            user.Score = CalculateClampedScore(oldScore, penalty);

            await _repo.AddLogAsync(new ReputationLog
            {
                UserID = annotatorId,
                OldScore = oldScore,
                NewScore = user.Score,
                ScoreChange = penalty,
                Reason = "Annotator khiếu nại sai (Dispute Rejected by Manager)",
                TaskID = taskId,
                RuleID = rules["Penalty_Annotator_Rejected_Dispute"].RuleID,
                CreatedAt = DateTime.UtcNow
            });

            var annoStat = await _context.AnnotatorStats.FirstOrDefaultAsync(s => s.UserID == annotatorId);
            if (annoStat != null)
            {
                annoStat.RejectDisputedTasksStreak++;
                int maxWrongStreak = rules["Max_Wrong_Disputed_Tasks_Streak"].Value;
                if (annoStat.RejectDisputedTasksStreak >= maxWrongStreak) user.IsActive = false;
            }

            if (user.Score <= 0) user.IsActive = false;
            await _context.SaveChangesAsync();
        }

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

        private int CalculateClampedScore(int currentScore, int delta)
        {
            int result = currentScore + delta;
            if (result > 100) return 100;
            if (result < 0) return 0;
            return result;
        }

        private async System.Threading.Tasks.Task CheckUserStatus(User user, Dictionary<string, ReputationRule> rules)
        {
            if (user.IsActive == false) user.IsActive = true;
            if (user.Score <= 0) user.IsActive = false;

            int streakLimit = rules["Max_Consecutive_Fails"].Value;
            var latestLogs = await _repo.GetLatestFailLogsAsync(user.UserID, streakLimit);

            if (latestLogs.Count == streakLimit && latestLogs.All(l => l.RuleID == rules["Penalty_Task_Fail"].RuleID))
            {
                user.IsActive = false;
            }
        }

        public async System.Threading.Tasks.Task HandleTaskRejectionAsync(Guid annotatorId, Guid reviewerId)
        {
            var annoStat = await _context.AnnotatorStats.FirstOrDefaultAsync(s => s.UserID == annotatorId);
            if (annoStat != null) annoStat.CurrentPerfectStreak = 0;

            var revStat = await _context.ReviewerStats.FirstOrDefaultAsync(s => s.UserID == reviewerId);
            if (revStat != null) revStat.CurrentPerfectRejectStreak++;

            await _context.SaveChangesAsync();
        }

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
            if (rule == null) return (false, "Không tìm thấy cấu hình luật này.");

            rule.Value = dto.Value;
            rule.Description = dto.Description;
            rule.IsActive = dto.IsActive;
            rule.UpdatedAt = DateTime.UtcNow;

            await _repo.SaveChangesAsync();
            return (true, $"Đã cập nhật thành công luật: {rule.RuleName}");
        }

        public async Task<AnnotatorStatsDto?> GetAnnotatorStatsAsync(Guid userId)
        {
            var stats = await _context.AnnotatorStats.FirstOrDefaultAsync(s => s.UserID == userId);
            if (stats == null) return null;

            return new AnnotatorStatsDto
            {
                TotalCompletedTasks = stats.TotalCompletedTasks,
                FirstTryApprovedTasks = stats.FirstTryApprovedTasks,
                TotalWorkingHours = Math.Round(stats.TotalWorkingHours, 2),
                AvgCompletionHours = Math.Round(stats.AvgCompletionHours, 2),
                CurrentPerfectStreak = stats.CurrentPerfectStreak,
                RejectDisputedTasksStreak = stats.RejectDisputedTasksStreak
            };
        }

        public async Task<ReviewerStatsDto?> GetReviewerStatsAsync(Guid userId)
        {
            var stats = await _context.ReviewerStats.FirstOrDefaultAsync(s => s.UserID == userId);
            if (stats == null) return null;

            return new ReviewerStatsDto
            {
                TotalReviewedTasks = stats.TotalReviewedTasks,
                TotalReviewHours = Math.Round(stats.TotalReviewHours, 2),
                AvgReviewHours = Math.Round(stats.AvgReviewHours, 2),
                DisputedTasksStreak = stats.DisputedTasksStreak,
                CurrentPerfectRejectStreak = stats.CurrentPerfectRejectStreak
            };
        }

        public async Task<IEnumerable<AnnotatorSummaryDto>> GetAllAnnotatorsPerformanceAsync()
        {
            return await _repo.GetAnnotatorsSummaryAsync();
        }

        public async Task<IEnumerable<ReviewerSummaryDto>> GetAllReviewersPerformanceAsync()
        {
            return await _repo.GetReviewersSummaryAsync();
        }
    }
}