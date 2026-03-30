using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using SWP_BE.Repositories;

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
            if (task.Status == Models.Task.TaskStatus.Approved || task.Status == Models.Task.TaskStatus.Rejected)
            {
                switch (task.CurrentRound)
                {//fix log ở đây (reason)
                    case 1: // Perfect
                        scoreDelta = rules["Reward_Perfect"].Value; // +20
                        reason = "Perfect: Hoàn thành ngay từ đầu";
                        appliedRuleId = rules["Reward_Perfect"].RuleID;
                        break;
                    case 2: // Sau sửa lần 1
                        scoreDelta = 0;
                        reason = "Approve sau sửa lần 1";
                        if (task.RateComplete > 95)
                        {
                            scoreDelta = 2;
                            reason += " (+2đ Bonus HighRate)";
                            appliedRuleId = rules["Bonus_HighRate"].RuleID;
                        }
                        break;
                    case 3: // Sau sửa lần 2
                        scoreDelta = rules["Penalty_Reject_2"].Value; // -5
                        reason = "Approve sau sửa lần 2";
                        appliedRuleId = rules["Penalty_Reject_2"].RuleID;
                        if (task.RateComplete > 95)
                        {
                            scoreDelta += 2;
                            reason += " (+2đ Bonus HighRate)";
                        }
                        break;
                    case 4: // Sau sửa lần 3
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

            // 2. CẬP NHẬT ANNOTATOR STATS
            var annoStat = await _context.AnnotatorStats.FirstOrDefaultAsync(s => s.UserID == userId);
            if (annoStat != null)
            {
                annoStat.TotalCompletedTasks++;
                annoStat.RejectDisputedTasksStreak = 0; // Reset khi hoàn thành task

                if (task.Status == Models.Task.TaskStatus.Approved && task.CurrentRound == 1)
                {
                    annoStat.FirstTryApprovedTasks++;
                    annoStat.CurrentPerfectStreak++;
                }

                // Tính thời gian làm việc (Giờ)
                double workHours = (DateTime.UtcNow - task.CreatedAt).TotalHours;
                annoStat.TotalWorkingHours += workHours;
                annoStat.AvgCompletionHours = annoStat.TotalWorkingHours / annoStat.TotalCompletedTasks;
            }

            // 3. CẬP NHẬT REVIEWER STATS (Khi Approve)

            var revStat = await _context.ReviewerStats.FirstOrDefaultAsync(s => s.UserID == task.ReviewerID.Value);
            if (revStat != null && task.Status == Models.Task.TaskStatus.Approved)
            {
                revStat.TotalReviewedTasks++;
                double reviewDuration = (DateTime.UtcNow - task.CreatedAt).TotalHours; // Giả định tính từ lúc tạo/giao
                revStat.TotalReviewHours += reviewDuration;
                revStat.AvgReviewHours = revStat.TotalReviewHours / revStat.TotalReviewedTasks;
            }

            await _repo.AddLogAsync(log);

            // --- CHECK SA THẢI ---
            await CheckUserStatus(user, rules);

            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// XỬ LÝ KHI REVIEWER THUA DISPUTE (Manager Accept khiếu nại)
        /// </summary>
        public async System.Threading.Tasks.Task HandleReviewerDisputeLossAsync(Guid reviewerId, Guid taskId)
        {
            var reviewer = await _repo.GetUserForUpdateAsync(reviewerId);
            if (reviewer == null) return;

            var rules = (await _repo.GetAllActiveRulesAsync()).ToDictionary(r => r.RuleName, r => r);

            // 1. TRỪ ĐIỂM REVIEWER (Rule 14: Penalty_Reviewer_False_Check = -10)
            int oldScore = reviewer.Score;
            int penalty = rules["Penalty_Reviewer_False_Check"].Value; // -10
            reviewer.Score = Math.Clamp(oldScore + penalty, 0, 100);

            // 2. LƯU LOG TRỪ ĐIỂM
            await _repo.AddLogAsync(new ReputationLog
            {
                UserID = reviewerId,
                OldScore = oldScore,
                NewScore = reviewer.Score,
                ScoreChange = reviewer.Score - oldScore,
                Reason = "Reviewer bắt lỗi sai (Dispute Accepted by Manager)",
                TaskID = taskId,
                RuleID = rules["Penalty_Reviewer_False_Check"].RuleID,
                CreatedAt = DateTime.UtcNow
            });

            // 3. CẬP NHẬT REVIEWER STATS
            var revStat = await _context.ReviewerStats.FirstOrDefaultAsync(s => s.UserID == reviewerId);
            if (revStat != null)
            {
                revStat.DisputedTasksStreak++; // Tăng chuỗi bị khiếu nại sai liên tiếp
                revStat.CurrentPerfectRejectStreak = 0; // Reset chuỗi phong độ (Reject đúng) về 0

                // 4. KIỂM TRA KHÓA TÀI KHOẢN (Rule 13: Max_Disputed_Tasks_Streak = 3)
                int maxStreak = rules["Max_Disputed_Tasks_Streak"].Value;
                if (revStat.DisputedTasksStreak >= maxStreak)
                {
                    reviewer.IsActive = false; // Khóa vì sai quá nhiều lần liên tiếp
                }
            }

            // Nếu điểm về 0 cũng khóa luôn
            if (reviewer.Score <= 0) reviewer.IsActive = false;

            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// XỬ LÝ KHI ANNOTATOR THUA DISPUTE (Manager Reject khiếu nại)
        /// </summary>
        public async System.Threading.Tasks.Task HandleAnnotatorDisputeLossAsync(Guid annotatorId, Guid taskId)
        {
            var user = await _repo.GetUserForUpdateAsync(annotatorId);
            if (user == null) return;

            var rules = (await _repo.GetAllActiveRulesAsync()).ToDictionary(r => r.RuleName, r => r);

            // 1. TRỪ ĐIỂM ANNOTATOR (Rule 12: Penalty_Annotator_Rejected_Dispute = -5)
            int oldScore = user.Score;
            int penalty = rules["Penalty_Annotator_Rejected_Dispute"].Value; // -5
            user.Score = Math.Clamp(oldScore + penalty, 0, 100);

            // 2. LƯU LOG TRỪ ĐIỂM
            await _repo.AddLogAsync(new ReputationLog
            {
                UserID = annotatorId,
                OldScore = oldScore,
                NewScore = user.Score,
                ScoreChange = user.Score - oldScore,
                Reason = "Annotator khiếu nại sai (Dispute Rejected by Manager)",
                TaskID = taskId,
                RuleID = rules["Penalty_Annotator_Rejected_Dispute"].RuleID,
                CreatedAt = DateTime.UtcNow
            });

            // 3. CẬP NHẬT ANNOTATOR STATS
            var annoStat = await _context.AnnotatorStats.FirstOrDefaultAsync(s => s.UserID == annotatorId);
            if (annoStat != null)
            {
                // Tăng chuỗi khiếu nại sai liên tiếp
                annoStat.RejectDisputedTasksStreak++;

                // 4. KIỂM TRA KHÓA TÀI KHOẢN 
                // Rule 16: Max_Wrong_Disputed_Tasks_Streak = 3
                int maxWrongStreak = rules["Max_Wrong_Disputed_Tasks_Streak"].Value;

                if (annoStat.RejectDisputedTasksStreak >= maxWrongStreak)
                {
                    user.IsActive = false; // "Bay màu" vì khiếu nại sai quá nhiều lần
                }
            }

            // Nếu điểm uy tín về 0 cũng cho nghỉ việc luôn
            if (user.Score <= 0) user.IsActive = false;

            await _context.SaveChangesAsync();
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
            // Nếu người dùng đang bị khóa nhưng điểm đã được cộng lại > 0 và ghi log thì mở khóa
            if (user.IsActive = false) user.IsActive = true;
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

        /// <summary>
        /// XỬ LÝ KHI REJECT (CẬP NHẬT CHUỖI PHONG ĐỘ)
        /// </summary>
        public async System.Threading.Tasks.Task HandleTaskRejectionAsync(Guid annotatorId, Guid reviewerId)
        {
            // Annotator: Đứt chuỗi Perfect
            var annoStat = await _context.AnnotatorStats.FirstOrDefaultAsync(s => s.UserID == annotatorId);
            if (annoStat != null) annoStat.CurrentPerfectStreak = 0;

            // Reviewer: Tăng chuỗi Reject đúng
            var revStat = await _context.ReviewerStats.FirstOrDefaultAsync(s => s.UserID == reviewerId);
            if (revStat != null) revStat.CurrentPerfectRejectStreak++;

            await _context.SaveChangesAsync();
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