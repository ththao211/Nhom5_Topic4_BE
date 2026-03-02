using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class ExportHistory
    {
        [Key]
        public Guid ExportID { get; set; }
        public string Format { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; } 
        public Guid ProjectID { get; set; }
        [ForeignKey("ProjectID")]
        public Project? Project { get; set; }

        public Guid ManagerID { get; set; }
        [ForeignKey("ManagerID")]
        public User? Manager { get; set; }
    }
}