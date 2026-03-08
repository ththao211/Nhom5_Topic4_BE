using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class ReputationLog
    {
        [Key]
        public int ReputationLogID { get; set; }

        public Guid UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        // --- CÁC CỘT MỚI ĐỂ THEO DÕI BIẾN ĐỘNG ---
        public int ScoreChange { get; set; }
        public int OldScore { get; set; }
        public int NewScore { get; set; }

        public string Reason { get; set; } = string.Empty;

        public Guid? TaskID { get; set; }
        [ForeignKey("TaskID")]
        public virtual Task? Task { get; set; }

        public int? RuleID { get; set; }
        [ForeignKey("RuleID")]
        public virtual ReputationRule? AppliedRule { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}