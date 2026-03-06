namespace SWP_BE.DTOs
{
    public class FeedbackDTO
    {
        public string Comment { get; set; }
        public string? ErrorRegion { get; set; } // bbox hoặc text segment
    }
}
