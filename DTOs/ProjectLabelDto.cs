using System.ComponentModel.DataAnnotations;

namespace SWP_BE.DTOs
{
    public class ProjectLabelDto
    {
        public int ProjectLabelID { get; set; }
        public Guid ProjectID { get; set; }
        public int LabelID { get; set; }
        public string CustomName { get; set; } = string.Empty;
        public string LabelName { get; set; } = string.Empty;
        public string DefaultColor { get; set; } = string.Empty;
    }

    public class ImportProjectLabelsDto
    {
        [Required(ErrorMessage = "Danh sách nhãn không được để trống")]
        public List<int> LabelIDs { get; set; } = new List<int>();
    }

    public class CreateCustomProjectLabelDto
    {
        [Required(ErrorMessage = "Tên nhãn tùy chỉnh là bắt buộc")]
        public string CustomName { get; set; } = string.Empty;

        public string DefaultColor { get; set; } = "#000000";
        public string Category { get; set; } = "Custom";

        public bool SaveToLibrary { get; set; } 
    }

    public class UpdateProjectLabelDto
    {
        [Required(ErrorMessage = "Tên nhãn tùy chỉnh là bắt buộc")]
        public string CustomName { get; set; } = string.Empty;
    }
}