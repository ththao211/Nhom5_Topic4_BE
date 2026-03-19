namespace SWP_BE.DTOs
{
    public class ReviewerDisputeDto
    {
        public Guid DisputeID { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public List<string> EvidenceImages { get; set; } = new();
    }
    public partial class FeedbackDTO
    {
        public string Comment { get; set; } = string.Empty;
        public string? ErrorRegion { get; set; }
    }
}
