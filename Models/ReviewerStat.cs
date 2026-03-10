using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class ReviewerStat
    {
        [Key]
        [ForeignKey("User")]
        public Guid UserID { get; set; }

        public virtual User? User { get; set; }

        // Tổng số task đã kiểm duyệt thành công
        public int TotalReviewedTasks { get; set; } = 0;

        // Thời gian kiểm duyệt trung bình (Từ lúc Task sang PendingReview đến lúc Approve/Reject)
        public double TotalReviewHours { get; set; } = 0;
        public double AvgReviewHours { get; set; } = 0;

        // Số task bị khiếu nại (Nếu bạn có làm thêm tính năng Dispute)
        public int DisputedTasks { get; set; } = 0;
    }
}