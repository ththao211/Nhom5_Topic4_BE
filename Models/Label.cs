using System.ComponentModel.DataAnnotations;

namespace SWP_BE.Models
{
    public class Label
    {
        [Key]
        public int LabelID { get; set; }
        public string DefaultColor { get; set; } = string.Empty;
        public string LabelName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}