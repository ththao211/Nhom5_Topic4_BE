namespace SWP_BE.DTOs
{
    public partial class FeedbackDTO
    {
        public string Comment { get; set; } = string.Empty;
        public string? ErrorRegion { get; set; }
    }
}
