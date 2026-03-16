using Microsoft.EntityFrameworkCore;
using SWP_BE.Models;
using System.Linq;

namespace SWP_BE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<SWP_BE.Models.Task> Tasks { get; set; }

        public DbSet<ProjectLabel> ProjectLabels { get; set; }
        public DbSet<DataItem> DataItems { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskItemDetail> TaskItemDetails { get; set; }
        public DbSet<ReviewHistory> ReviewHistories { get; set; }
        public DbSet<ReviewComment> ReviewComments { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<ReputationLog> ReputationLogs { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<ExportHistory> ExportHistories { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ReputationRule> ReputationRules { get; set; }
        public DbSet<AnnotatorStat> AnnotatorStats { get; set; }
        public DbSet<ReviewerStat> ReviewerStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasOne(a => a.Performer)
                      .WithMany()
                      .HasForeignKey(a => a.PerformedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.TargetUser)
                      .WithMany()
                      .HasForeignKey(a => a.TargetUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ReputationRule>().HasData
            (
                // --- Nhóm điểm thưởng/phạt theo lần Reject ---
                new ReputationRule { RuleID = 1, RuleName = "Reward_Perfect", Value = 20, Category = "Reward", Description = "Hoàn thành ngay lần đầu (0 reject)" },
                new ReputationRule { RuleID = 2, RuleName = "Bonus_HighRate", Value = 2, Category = "Bonus", Description = "Thưởng thêm nếu RateComplete > 95%" },
                new ReputationRule { RuleID = 3, RuleName = "Penalty_Reject_2", Value = -5, Category = "Penalty", Description = "Trừ điểm khi Approve ở lần sửa 2" },
                new ReputationRule { RuleID = 4, RuleName = "Penalty_Reject_3", Value = -10, Category = "Penalty", Description = "Trừ điểm khi Approve ở lần sửa 3" },
                new ReputationRule { RuleID = 5, RuleName = "Penalty_Task_Fail", Value = -20, Category = "Penalty", Description = "Task bị Fail (Reject lần 4)" },

                // --- Nhóm ngưỡng điểm để Manager Assign Task (Giữ nguyên hoặc chỉnh theo hình) ---
                new ReputationRule { RuleID = 6, RuleName = "High_Threshold", Value = 50, Category = "Threshold", Description = "Ngưỡng >= 50đ" },
                new ReputationRule { RuleID = 7, RuleName = "Low_Threshold", Value = 20, Category = "Threshold", Description = "Ngưỡng 20 - 50đ" },
                new ReputationRule { RuleID = 8, RuleName = "Max_Task_High", Value = 3, Category = "Limit", Description = "Max 3 task" },
                new ReputationRule { RuleID = 9, RuleName = "Max_Task_Normal", Value = 2, Category = "Limit", Description = "Max 2 task" },
                new ReputationRule { RuleID = 10, RuleName = "Max_Task_Warning", Value = 1, Category = "Limit", Description = "Max 1 task" },
                new ReputationRule { RuleID = 11, RuleName = "Max_Consecutive_Fails", Value = 3, Category = "Limit", Description = "Số task Fail liên tiếp để bị khóa tài khoản" }
            );
            // Cấu hình quan hệ 1-1 cho AnnotatorStat
            modelBuilder.Entity<User>()
                .HasOne(u => u.AnnotatorStat)
                .WithOne(s => s.User)
                .HasForeignKey<AnnotatorStat>(s => s.UserID);

            // Cấu hình quan hệ 1-1 cho ReviewerStat
            modelBuilder.Entity<User>()
                .HasOne(u => u.ReviewerStat)
                .WithOne(s => s.User)
                .HasForeignKey<ReviewerStat>(s => s.UserID);
        }
    }
}