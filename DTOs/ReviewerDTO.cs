using System;
using System.Collections.Generic;

namespace SWP_BE.DTOs
{
    public class ReviewerDisputeDto
    {
        public Guid DisputeID { get; set; }
        public Guid TaskID { get; set; } // Đã thêm TaskID
        public string TaskName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;

        // --- 3 TRƯỜNG DỮ LIỆU ĐÃ THÊM CHO FRONTEND ---
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<string> EvidenceImages { get; set; } = new();
    }

    public partial class FeedbackDTO
    {
        public string Comment { get; set; } = string.Empty;
        public string? ErrorRegion { get; set; }
    }
}