using System.ComponentModel.DataAnnotations;

namespace SWP_BE.Models
{
    public class ReputationRule
    {
        [Key]
        public int RuleID { get; set; }

        [Required]
        public string RuleName { get; set; } = string.Empty;

        public int Value { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}