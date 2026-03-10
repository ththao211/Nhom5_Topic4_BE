using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class User
    {
        public enum UserRole
        {
            Admin = 1,
            Manager = 2,
            Annotator = 3,
            Reviewer = 4
        }

        [Key]
        public Guid UserID { get; set; } = Guid.NewGuid();

        [Required]
        public string UserName { get; set; } = string.Empty;

        // [ĐÃ SỬA]: Bỏ [Required] và thêm dấu ? (nullable) để cho phép login bằng Google
        public string? Password { get; set; }

        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = 0;
        public string Email { get; set; } = string.Empty;
        //Lưu UID từ Firebase trả về để tăng cường bảo mật
        public string? FirebaseUid { get; set; }
        // Lưu Avatar Google để hiện lên UI cho đẹp
        public string? AvatarUrl { get; set; }

        public string? Expertise { get; set; }
        public int Score { get; set; }
        public int CurrentTaskCount { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<ReputationLog> ReputationLogs { get; set; } = new List<ReputationLog>();

        [InverseProperty("Manager")]
        public ICollection<Project>? ManagedProjects { get; set; }

        [InverseProperty("Annotator")]
        public ICollection<Task>? AnnotatorTasks { get; set; }

        [InverseProperty("Reviewer")]
        public ICollection<Task>? ReviewerTasks { get; set; }

        public virtual AnnotatorStat? AnnotatorStat { get; set; }
        public virtual ReviewerStat? ReviewerStat { get; set; }
    }
}