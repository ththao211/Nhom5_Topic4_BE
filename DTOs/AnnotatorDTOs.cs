namespace SWP_BE.DTOs
{
    public class AnnotatorTaskDto
    {
        public Guid TaskID { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int CurrentRound { get; set; }
    }

    public class TaskDetailDto : AnnotatorTaskDto
    {
        public List<TaskItemDto> TaskItems { get; set; } = new();
    }

    public class TaskItemDto
    {
        public Guid ItemID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool IsFlagged { get; set; }
        public string? AnnotationData { get; set; } // Lấy từ TaskItemDetail
        public string? Content { get; set; }
    }

    public class SaveAnnotationDto
    {
        public string AnnotationData { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class DisputeRequestDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class ReputationResponseDto
    {
        public int CurrentScore { get; set; }
        public List<ReputationLogDto> Logs { get; set; } = new();
    }

    public class ReputationLogDto
    {
        public int ScoreChange { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}