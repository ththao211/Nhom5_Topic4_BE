using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class Project
    {
        [Key]
        public Guid ProjectID { get; set; } 
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? Deadline { get; set; }
        public string ProjectType { get; set; } = string.Empty;
        public string GuidelineUrl { get; set; } = string.Empty;
        public Guid ManagerID { get; set; }
        [ForeignKey("ManagerID")]
        public User? Manager { get; set; }
        public virtual ICollection<DataItem> DataItems { get; set; } = new List<DataItem>();
        public virtual ICollection<ProjectLabel>? ProjectLabels { get; set; } 
        public virtual ICollection<Task>? Tasks { get; set; }
    }
}
