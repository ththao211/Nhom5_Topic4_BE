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

        // Tiêu chí 5: Kinh nghiệm (Tổng số task đã kiểm duyệt)
        public int TotalReviewedTasks { get; set; } = 0;

        // Tiêu chí 3: Tỷ lệ duyệt lần đầu (Check chuẩn, không bị Dispute hoặc không cần Annotator sửa lại nhiều)
        public int FirstTryApprovedTasks { get; set; } = 0;

        // Tiêu chí 6: Thời gian kiểm duyệt trung bình
        public double TotalReviewHours { get; set; } = 0;
        public double AvgReviewHours { get; set; } = 0;

        // Theo dõi Khiếu nại
        public int DisputedTasks { get; set; } = 0;

        // Tiêu chí 7: Phong độ (Số task duyệt chuẩn liên tiếp không bị khiếu nại thắng)
        public int CurrentPerfectStreak { get; set; } = 0;
    }
}