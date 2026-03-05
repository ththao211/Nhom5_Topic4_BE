using System;
using System.Collections.Generic;

namespace SWP_BE.DTOs
{
    // THÊM MỚI: Class chứa thông tin Tên và Màu của nhãn
    public class LabelInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class AnnotatorTaskDto
    {
        public Guid TaskID { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int CurrentRound { get; set; }
    }

    public class TaskDetailDto : AnnotatorTaskDto
    {
        public List<TaskItemDto> TaskItems { get; set; } = new();

        public List<LabelInfoDto> AvailableLabels { get; set; } = new();
    }

    public class AnnotationDetailDto
    {
        // Dữ liệu tọa độ (x, y, width, height) dạng JSON string
        public string AnnotationData { get; set; } = string.Empty;

        // Tên của Label mà người dùng đã chọn (ví dụ: "Car")
        public string Content { get; set; } = string.Empty;

        // Loại công cụ vẽ (mặc định là BoundingBox)
        public string Field { get; set; } = "BoundingBox";
    }

    public class TaskItemDto
    {
        public Guid ItemID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool IsFlagged { get; set; }
        public List<AnnotationDetailDto> Annotations { get; set; } = new();
    }

    public class SaveAnnotationDto
    {
        public List<AnnotationDetailDto> Annotations { get; set; } = new();
    }

    public class DisputeRequestDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class ReputationResponseDto
    {
        public int CurrentScore { get; set; }
        public List<ReputationLogDto> Logs { get; set; } = new();
    }

    public class ReputationLogDto
    {
        public int ScoreChange { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}