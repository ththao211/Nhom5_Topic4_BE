using System.ComponentModel.DataAnnotations;

namespace SWP_BE.DTOs
{
    // DTO cho API 1: Dữ liệu chưa gán
    public class UnassignedDataItemDto
    {
        public Guid DataID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
    }

    // DTO cho API 2: Tạo Task mới
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Tên task là bắt buộc")]
        public string TaskName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh sách dữ liệu không được để trống")]
        public List<Guid> DataIDs { get; set; } = new List<Guid>();

        public DateTime? Deadline { get; set; }
    }

    // DTO cho API 3: Giao nhân sự
    public class AssignTaskDto
    {
        public Guid? AnnotatorID { get; set; }
        public Guid? ReviewerID { get; set; }
    }

    // DTO cho API 4: Lấy danh sách task tiến độ
    public class TaskProgressDto
    {
        public Guid TaskID { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double RateComplete { get; set; }
        public int RejectCount { get; set; }
        public DateTime? Deadline { get; set; }
        public Guid? AnnotatorID { get; set; }
        public Guid? ReviewerID { get; set; }
        public int TotalItems { get; set; } // Số lượng ảnh trong task
    }

    // DTO cho API 5: Sửa deadline
    public class UpdateDeadlineDto
    {
        [Required]
        public DateTime Deadline { get; set; }
    }
}