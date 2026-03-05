using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    [Table("Tasks")]
    public class Task
    {
        // Định nghĩa các trạng thái bắt buộc phải có của 1 Task
        public enum TaskStatus
        {
            New = 1,
            InProgress = 2,
            PendingReview = 3,
            Approved = 4,
            Rejected = 5,
            Fail = 6 // Bị reject 3 lần
        }

        [Key]
        public Guid TaskID { get; set; }
        public string TaskName { get; set; } = string.Empty;

        // [THAY ĐỔI] Dùng Enum thay vì string
        public TaskStatus Status { get; set; } = TaskStatus.New;
        public int RejectCount { get; set; }
        public double RateComplete { get; set; } // Tính % hoàn thành
        public DateTime Deadline { get; set; }
        public int CurrentRound { get; set; } // Vòng làm việc (Sửa lần 1, lần 2...)
        public double SubmissionRate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public Guid ProjectID { get; set; }
        [ForeignKey("ProjectID")]
        public Project? Project { get; set; }
        public Guid? AnnotatorID { get; set; }
        [ForeignKey("AnnotatorID")]
        public User? Annotator { get; set; }
        public Guid? ReviewerID { get; set; }
        [ForeignKey("ReviewerID")]
        public User? Reviewer { get; set; }
        public virtual ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
    }
}