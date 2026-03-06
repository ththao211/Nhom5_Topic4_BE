using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class DataItem
    {
        [Key]
        public Guid DataID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        // Dùng long để lưu Byte (Ví dụ: 1048576 thay vì "1 MB")
        public long FileSizeBytes { get; set; }
        public string FileType { get; set; } = string.Empty;
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool IsAssigned { get; set; } = false;
        public Guid ProjectID { get; set; }
        [ForeignKey("ProjectID")]
        public Project? Project { get; set; }
    }
}