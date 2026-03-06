using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP_BE.Models
{
    public class Dispute
    {
        [Key]
        public Guid DisputeID { get; set; } = Guid.NewGuid();

        // Nội dung khiếu nại của Annotator
        public string Reason { get; set; } = string.Empty;

        // Phản hồi của Manager
        public string ManagerComment { get; set; } = string.Empty;

        // Trạng thái: Pending (Đang chờ), Approved (Chấp nhận), Rejected (Từ chối)
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // Liên kết với Task bị khiếu nại
        public Guid TaskID { get; set; }
        [ForeignKey("TaskID")]
        public Task? Task { get; set; }

        // Liên kết với người gửi khiếu nại (Annotator)
        public Guid UserID { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }
    }
}