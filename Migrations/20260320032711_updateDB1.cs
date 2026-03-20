using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWP_BE.Migrations
{
    public partial class updateDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. CẬP NHẬT BẢNG TASKS: Xóa 2 cột đã bỏ trong Model
            migrationBuilder.DropColumn(name: "FirstRate", table: "Tasks");
            migrationBuilder.DropColumn(name: "SubmissionRate", table: "Tasks");

            // 2. CẬP NHẬT BẢNG ANNOTATORSTATS: Thêm 1 cột mới
            migrationBuilder.AddColumn<int>(
                name: "RejectDisputedTasksStreak",
                table: "AnnotatorStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // 3. CẬP NHẬT BẢNG REVIEWERSTATS: Xóa 3 cột cũ, thêm 2 cột mới
            migrationBuilder.DropColumn(name: "DisputedTasks", table: "ReviewerStats");
            migrationBuilder.DropColumn(name: "FirstTryApprovedTasks", table: "ReviewerStats");
            migrationBuilder.DropColumn(name: "CurrentPerfectStreak", table: "ReviewerStats");

            migrationBuilder.AddColumn<int>(
                name: "DisputedTasksStreak",
                table: "ReviewerStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentPerfectRejectStreak",
                table: "ReviewerStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // 4. THÊM DATA BẢNG REPUTATIONRULES: Chỉ thêm các Rule mới (12 -> 16), tránh lỗi trùng Rule 1-11 đã có trên DB
            migrationBuilder.InsertData(
                table: "ReputationRules",
                columns: new[] { "RuleID", "Category", "Description", "IsActive", "RuleName", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 12, "Penalty", "Annotator khiếu nại sai (Dispute Rejected)", true, "Penalty_Annotator_Rejected_Dispute", DateTime.UtcNow, -5 },
                    { 13, "Limit", "Số lần task bị Disputed liên tục để Reviewer bị khóa tài khoản", true, "Max_Disputed_Tasks_Streak", DateTime.UtcNow, 3 },
                    { 14, "Penalty", "Reviewer bắt lỗi sai (Dispute lost)", true, "Penalty_Reviewer_False_Check", DateTime.UtcNow, -10 },
                    { 15, "Reward", "Thưởng Reviewer reject 5 task liên tiếp không sai", true, "Reward_Reviewer_Perfect_Reject_Streak", DateTime.UtcNow, 10 },
                    { 16, "Limit", "Số lần task bị Disputed sai liên tục để Annotator bị khóa tài khoản", true, "Max_Wrong_Disputed_Tasks_Streak", DateTime.UtcNow, 3 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Để trống để đảm bảo an toàn, không vô tình làm mất data nếu có lỡ chạy Rollback
        }
    }
}