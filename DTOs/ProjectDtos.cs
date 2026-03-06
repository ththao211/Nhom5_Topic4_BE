using System.ComponentModel.DataAnnotations;

namespace SWP_BE.DTOs
{
    public class ExportRequest
    {
        public string Formats { get; set; }
    }
    public class CreateProjectDto
    {
        [Required(ErrorMessage = "Tên dự án là bắt buộc")]
        [MaxLength(200)]
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Topic { get; set; }
        public string? ProjectType { get; set; }
        public string? GuidelineUrl { get; set; } // Fix lỗi CS1061
    }

    public class UpdateProjectDto
    {
        [Required(ErrorMessage = "Tên dự án là bắt buộc")]
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Topic { get; set; }
        public string? ProjectType { get; set; }
        public string? GuidelineUrl { get; set; }
    }

    public class ProjectResponseDto
    {
        public Guid ProjectID { get; set; } // Chuyển sang Guid
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Topic { get; set; }
        public string? ProjectType { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? GuidelineUrl { get; set; }
        public int TotalDataItems { get; set; }
        public List<DataItemDto> DataItems { get; set; } = new List<DataItemDto>();
    }

    public class DataItemDto
    {
        public Guid DataID { get; set; } // Chuyển sang Guid
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }

    public class UploadDataDto
    {
        [Required]
        public List<string> FileUrls { get; set; } = new List<string>();
        public string FileType { get; set; } = "image";
    }

    public class SplitTaskDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int NumberOfItemsPerTask { get; set; }
        public string TaskPrefix { get; set; } = "Task";
    }

    public class SplitTaskResultDto
    {
        public Guid TaskId { get; set; } // Chuyển sang Guid
        public int ItemCount { get; set; }
    }
}