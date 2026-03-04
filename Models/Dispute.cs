using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class Dispute
    {
        [Key]
        public int DisputeID { get; set; }

        public string Reason { get; set; } = string.Empty;
        public string ManagerComment { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ResolvedAt { get; set; }
        public Guid TaskID { get; set; }
        [ForeignKey("TaskID")]
        public Task? Task { get; set; }
    }
}