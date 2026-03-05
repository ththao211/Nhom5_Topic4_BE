using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class TaskItemDetail
    {
        [Key]
        public int IDDetail { get; set; }

        // Backend PHẢI lưu chuỗi JSON từ Frontend gửi lên vào đây.
        // Ví dụ: "{\"x\": 0.45, \"y\": 0.5, \"w\": 0.2, \"h\": 0.1, \"labelId\": 5}"
        public string AnnotationData { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty; // Ghi chú của Annotator
        public bool IsApproved { get; set; }

        public string Field { get; set; } = string.Empty;

        public Guid TaskItemID { get; set; }
        [ForeignKey("TaskItemID")]
        public TaskItem? TaskItem { get; set; }
    }
}