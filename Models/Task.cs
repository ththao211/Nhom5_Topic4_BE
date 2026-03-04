using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    [Table("Tasks")]
    public class Task
    {
        [Key]
        public Guid TaskID { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public int RejectCount { get; set; }
        public double RateComplete { get; set; }
        public DateTime Deadline { get; set; }
        public int CurrentRound { get; set; }
        public double SubmissionRate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Foreign Keys
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