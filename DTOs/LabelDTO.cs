using System.ComponentModel.DataAnnotations;

namespace SWP_BE.DTOs
{
    public class LabelDto
    {
        public int LabelID { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public string DefaultColor { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class CreateLabelDto
    {
        [Required(ErrorMessage = "Tên nhãn là bắt buộc")]
        public string LabelName { get; set; } = string.Empty;

        public string DefaultColor { get; set; } = "#000000"; // Gán màu đen làm mặc định nếu không truyền

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public string Category { get; set; } = string.Empty;
    }

    public class UpdateLabelDto
    {
        public string? LabelName { get; set; }
        public string? DefaultColor { get; set; }
        public string? Category { get; set; }
    }
}