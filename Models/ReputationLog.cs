using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class ReputationLog
    {
        [Key]
        public int ReputationLogID { get; set; }
        public int ScoreChange { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Guid UserID { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }
        public Guid? TaskID { get; set; }
        [ForeignKey("TaskID")]
        public Task? Task { get; set; }
    }
}