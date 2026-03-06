using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class ActivityLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Action { get; set; } = string.Empty;

        public Guid? PerformedBy { get; set; }

        [ForeignKey("PerformedBy")]
        public virtual User? Performer { get; set; }

        public Guid? TargetUserId { get; set; }

        [ForeignKey("TargetUserId")]
        public virtual User? TargetUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}