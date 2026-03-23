namespace SWP_BE.DTOs
{
    public class SuggestionDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Expertise { get; set; } = string.Empty;
        public int ReputationScore { get; set; }
        public int CurrentTaskCount { get; set; }

        // Các thông số để Manager check lại
        public double FirstTryRate { get; set; }
        public int Experience { get; set; } // TotalCompleted hoặc TotalReviewed
        public double AvgHours { get; set; }
        public int PerfectStreak { get; set; }
        public string SuggestionNote { get; set; } = string.Empty;
    }
}