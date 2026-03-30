
public class ReputationRuleDto
{
    public int RuleID { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateRuleDto
{
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }


    public class AnnotatorStatsDto
    {
        public int TotalCompletedTasks { get; set; }
        public int FirstTryApprovedTasks { get; set; }
        public double TotalWorkingHours { get; set; }
        public double AvgCompletionHours { get; set; }
        public int CurrentPerfectStreak { get; set; }
        public int RejectDisputedTasksStreak { get; set; }
    }

    public class ReviewerStatsDto
    {
        public int TotalReviewedTasks { get; set; }
        public double TotalReviewHours { get; set; }
        public double AvgReviewHours { get; set; }
        public int DisputedTasksStreak { get; set; }
        public int CurrentPerfectRejectStreak { get; set; }
    }
    public class AnnotatorSummaryDto
    {
        public Guid UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int Score { get; set; }
        public bool IsActive { get; set; }
        public int TotalCompletedTasks { get; set; }
        public int CurrentPerfectStreak { get; set; }
        public double AvgCompletionHours { get; set; }
    }

    public class ReviewerSummaryDto
    {
        public Guid UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int Score { get; set; }
        public bool IsActive { get; set; }
        public int TotalReviewedTasks { get; set; }
        public int DisputedTasksStreak { get; set; } // Chuỗi bị khiếu nại sai
        public double AvgReviewHours { get; set; }
    }
}