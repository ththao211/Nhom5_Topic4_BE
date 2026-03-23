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

        // Tiêu chí 6: Thời gian kiểm duyệt trung bình (Tổng thời gian kiểm duyệt / Tổng số task đã kiểm duyệt)
        public double TotalReviewHours { get; set; } = 0;
        public double AvgReviewHours { get; set; } = 0;

        // Theo dõi chuỗi Khiếu nại liên tiếp (Disputed Tasks Streak) để áp dụng hình phạt nếu vượt ngưỡng (3)
        public int DisputedTasksStreak { get; set; } = 0;

        // Tiêu chí 7: Phong độ (số thắng khiếu nại liên tiếp hoặc reject task liên tiếp không bị khiếu nại)
        public int CurrentPerfectRejectStreak { get; set; } = 0;
    }
}