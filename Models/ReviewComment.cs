using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class ReviewComment
    {
        [Key]
        public int CommentID { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string ErrorRegion { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int HistoryID { get; set; }
        [ForeignKey("HistoryID")]
        public ReviewHistory? ReviewHistory { get; set; }
    }
}