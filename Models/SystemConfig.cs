using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class SystemConfig
    {
        [Key]
        public int ConfigID { get; set; }
        public string Value { get; set; } = string.Empty;
        public int MaxProjectStorageMB { get; set; }
        public string AllowedFileTypes { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public Guid AdminID { get; set; }
        [ForeignKey("AdminID")]
        public User? Admin { get; set; }
    }
}