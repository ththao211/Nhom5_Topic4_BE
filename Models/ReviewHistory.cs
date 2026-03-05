using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class ReviewHistory
    {
        [Key]
        public int HistoryID { get; set; }
        public DateTime ReviewAt { get; set; } = DateTime.Now;
        public string FinalResult { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public Guid TaskID { get; set; }
        [ForeignKey("TaskID")]
        public Task? Task { get; set; }
        public Guid ReviewerID { get; set; }
        [ForeignKey("ReviewerID")]
        public User? Reviewer { get; set; }
    }
}