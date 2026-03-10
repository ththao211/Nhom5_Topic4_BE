using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class AnnotatorStat
    {
        [Key]
        [ForeignKey("User")]
        public Guid UserID { get; set; } // Vừa là PK vừa là FK trỏ về User

        public virtual User? User { get; set; }

        // Tiêu chí 6: Kinh nghiệm
        public int TotalCompletedTasks { get; set; } = 0;

        // Tiêu chí 3: Tỷ lệ duyệt lần đầu
        public int FirstTryApprovedTasks { get; set; } = 0;

        // Tiêu chí 5: Thời gian hoàn thành trung bình
        public double TotalWorkingHours { get; set; } = 0;
        public double AvgCompletionHours { get; set; } = 0;

        // Tiêu chí 8: Phong độ (Số task perfect liên tiếp)
        public int CurrentPerfectStreak { get; set; } = 0;
    }
}