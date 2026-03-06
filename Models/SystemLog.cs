using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class SystemLog
    {
        [Key]
        public int LogID { get; set; }

        public string ActionType { get; set; } = string.Empty;

        public string TargetID { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string EntityType { get; set; } = string.Empty; 

        public Guid UserID { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }
    }
}