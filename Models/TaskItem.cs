using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class TaskItem
    {
        [Key]
        public Guid ItemID { get; set; }
        public bool IsFlagged { get; set; }
        public Guid TaskID { get; set; }
        [ForeignKey("TaskID")]
        public SWP_BE.Models.Task? Task { get; set; }
        public Guid DataID { get; set; }
        [ForeignKey("DataID")]
        public DataItem? DataItem { get; set; }
        public virtual ICollection<TaskItemDetail> TaskItemDetails { get; set; } = new List<TaskItemDetail>();
    }
}