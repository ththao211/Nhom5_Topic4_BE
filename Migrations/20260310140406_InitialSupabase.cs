using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SWP_BE.Migrations
{
    /// <inheritdoc />
    public partial class InitialSupabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Labels",
                columns: table => new
                {
                    LabelID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DefaultColor = table.Column<string>(type: "text", nullable: false),
                    LabelName = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labels", x => x.LabelID);
                });

            migrationBuilder.CreateTable(
                name: "ReputationRules",
                columns: table => new
                {
                    RuleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RuleName = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReputationRules", x => x.RuleID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FirebaseUid = table.Column<string>(type: "text", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    Expertise = table.Column<string>(type: "text", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    CurrentTaskCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    PerformedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_Users_PerformedBy",
                        column: x => x.PerformedBy,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_Users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectID = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectType = table.Column<string>(type: "text", nullable: false),
                    GuidelineUrl = table.Column<string>(type: "text", nullable: false),
                    ManagerID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectID);
                    table.ForeignKey(
                        name: "FK_Projects_Users_ManagerID",
                        column: x => x.ManagerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigs",
                columns: table => new
                {
                    ConfigID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "text", nullable: false),
                    MaxProjectStorageMB = table.Column<int>(type: "integer", nullable: false),
                    AllowedFileTypes = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdminID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigs", x => x.ConfigID);
                    table.ForeignKey(
                        name: "FK_SystemConfigs_Users_AdminID",
                        column: x => x.AdminID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    TargetID = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_SystemLogs_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataItems",
                columns: table => new
                {
                    DataID = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    IsAssigned = table.Column<bool>(type: "boolean", nullable: false),
                    ProjectID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataItems", x => x.DataID);
                    table.ForeignKey(
                        name: "FK_DataItems_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExportHistories",
                columns: table => new
                {
                    ExportID = table.Column<Guid>(type: "uuid", nullable: false),
                    Format = table.Column<string>(type: "text", nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProjectID = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagerID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportHistories", x => x.ExportID);
                    table.ForeignKey(
                        name: "FK_ExportHistories_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExportHistories_Users_ManagerID",
                        column: x => x.ManagerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectLabels",
                columns: table => new
                {
                    ProjectLabelID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomName = table.Column<string>(type: "text", nullable: false),
                    ProjectID = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectLabels", x => x.ProjectLabelID);
                    table.ForeignKey(
                        name: "FK_ProjectLabels_Labels_LabelID",
                        column: x => x.LabelID,
                        principalTable: "Labels",
                        principalColumn: "LabelID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectLabels_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    TaskID = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskName = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RejectCount = table.Column<int>(type: "integer", nullable: false),
                    RateComplete = table.Column<double>(type: "double precision", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentRound = table.Column<int>(type: "integer", nullable: false),
                    SubmissionRate = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectID = table.Column<Guid>(type: "uuid", nullable: false),
                    AnnotatorID = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewerID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.TaskID);
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_AnnotatorID",
                        column: x => x.AnnotatorID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_ReviewerID",
                        column: x => x.ReviewerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Disputes",
                columns: table => new
                {
                    DisputeID = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    ManagerComment = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TaskID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disputes", x => x.DisputeID);
                    table.ForeignKey(
                        name: "FK_Disputes_Tasks_TaskID",
                        column: x => x.TaskID,
                        principalTable: "Tasks",
                        principalColumn: "TaskID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Disputes_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReputationLogs",
                columns: table => new
                {
                    ReputationLogID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    ScoreChange = table.Column<int>(type: "integer", nullable: false),
                    OldScore = table.Column<int>(type: "integer", nullable: false),
                    NewScore = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    TaskID = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleID = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReputationLogs", x => x.ReputationLogID);
                    table.ForeignKey(
                        name: "FK_ReputationLogs_ReputationRules_RuleID",
                        column: x => x.RuleID,
                        principalTable: "ReputationRules",
                        principalColumn: "RuleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReputationLogs_Tasks_TaskID",
                        column: x => x.TaskID,
                        principalTable: "Tasks",
                        principalColumn: "TaskID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReputationLogs_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewHistories",
                columns: table => new
                {
                    HistoryID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinalResult = table.Column<string>(type: "text", nullable: false),
                    Field = table.Column<string>(type: "text", nullable: false),
                    TaskID = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewHistories", x => x.HistoryID);
                    table.ForeignKey(
                        name: "FK_ReviewHistories_Tasks_TaskID",
                        column: x => x.TaskID,
                        principalTable: "Tasks",
                        principalColumn: "TaskID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewHistories_Users_ReviewerID",
                        column: x => x.ReviewerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskItems",
                columns: table => new
                {
                    ItemID = table.Column<Guid>(type: "uuid", nullable: false),
                    IsFlagged = table.Column<bool>(type: "boolean", nullable: false),
                    TaskID = table.Column<Guid>(type: "uuid", nullable: false),
                    DataID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskItems", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_TaskItems_DataItems_DataID",
                        column: x => x.DataID,
                        principalTable: "DataItems",
                        principalColumn: "DataID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskItems_Tasks_TaskID",
                        column: x => x.TaskID,
                        principalTable: "Tasks",
                        principalColumn: "TaskID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewComments",
                columns: table => new
                {
                    CommentID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    ErrorRegion = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HistoryID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewComments", x => x.CommentID);
                    table.ForeignKey(
                        name: "FK_ReviewComments_ReviewHistories_HistoryID",
                        column: x => x.HistoryID,
                        principalTable: "ReviewHistories",
                        principalColumn: "HistoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskItemDetails",
                columns: table => new
                {
                    IDDetail = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnnotationData = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Field = table.Column<string>(type: "text", nullable: false),
                    TaskItemID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskItemDetails", x => x.IDDetail);
                    table.ForeignKey(
                        name: "FK_TaskItemDetails_TaskItems_TaskItemID",
                        column: x => x.TaskItemID,
                        principalTable: "TaskItems",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ReputationRules",
                columns: new[] { "RuleID", "Category", "Description", "IsActive", "RuleName", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, "Reward", "Hoàn thành ngay lần đầu (0 reject)", true, "Reward_Perfect", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8081), 20 },
                    { 2, "Bonus", "Thưởng thêm nếu RateComplete > 95%", true, "Bonus_HighRate", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8091), 2 },
                    { 3, "Penalty", "Trừ điểm khi Approve ở lần sửa 2", true, "Penalty_Reject_2", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8093), -5 },
                    { 4, "Penalty", "Trừ điểm khi Approve ở lần sửa 3", true, "Penalty_Reject_3", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8094), -10 },
                    { 5, "Penalty", "Task bị Fail (Reject lần 4)", true, "Penalty_Task_Fail", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8095), -20 },
                    { 6, "Threshold", "Ngưỡng >= 50đ", true, "High_Threshold", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8096), 50 },
                    { 7, "Threshold", "Ngưỡng 20 - 50đ", true, "Low_Threshold", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8097), 20 },
                    { 8, "Limit", "Max 3 task", true, "Max_Task_High", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8098), 3 },
                    { 9, "Limit", "Max 2 task", true, "Max_Task_Normal", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8099), 2 },
                    { 10, "Limit", "Max 1 task", true, "Max_Task_Warning", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8100), 1 },
                    { 11, "Limit", "Số task Fail liên tiếp để bị khóa tài khoản", true, "Max_Consecutive_Fails", new DateTime(2026, 3, 10, 21, 4, 5, 866, DateTimeKind.Local).AddTicks(8101), 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_PerformedBy",
                table: "ActivityLogs",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_TargetUserId",
                table: "ActivityLogs",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataItems_ProjectID",
                table: "DataItems",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_TaskID",
                table: "Disputes",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_UserID",
                table: "Disputes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ExportHistories_ManagerID",
                table: "ExportHistories",
                column: "ManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_ExportHistories_ProjectID",
                table: "ExportHistories",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLabels_LabelID",
                table: "ProjectLabels",
                column: "LabelID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLabels_ProjectID",
                table: "ProjectLabels",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ManagerID",
                table: "Projects",
                column: "ManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLogs_RuleID",
                table: "ReputationLogs",
                column: "RuleID");

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLogs_TaskID",
                table: "ReputationLogs",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLogs_UserID",
                table: "ReputationLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComments_HistoryID",
                table: "ReviewComments",
                column: "HistoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHistories_ReviewerID",
                table: "ReviewHistories",
                column: "ReviewerID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHistories_TaskID",
                table: "ReviewHistories",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigs_AdminID",
                table: "SystemConfigs",
                column: "AdminID");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_UserID",
                table: "SystemLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItemDetails_TaskItemID",
                table: "TaskItemDetails",
                column: "TaskItemID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_DataID",
                table: "TaskItems",
                column: "DataID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_TaskID",
                table: "TaskItems",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AnnotatorID",
                table: "Tasks",
                column: "AnnotatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectID",
                table: "Tasks",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ReviewerID",
                table: "Tasks",
                column: "ReviewerID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "Disputes");

            migrationBuilder.DropTable(
                name: "ExportHistories");

            migrationBuilder.DropTable(
                name: "ProjectLabels");

            migrationBuilder.DropTable(
                name: "ReputationLogs");

            migrationBuilder.DropTable(
                name: "ReviewComments");

            migrationBuilder.DropTable(
                name: "SystemConfigs");

            migrationBuilder.DropTable(
                name: "SystemLogs");

            migrationBuilder.DropTable(
                name: "TaskItemDetails");

            migrationBuilder.DropTable(
                name: "Labels");

            migrationBuilder.DropTable(
                name: "ReputationRules");

            migrationBuilder.DropTable(
                name: "ReviewHistories");

            migrationBuilder.DropTable(
                name: "TaskItems");

            migrationBuilder.DropTable(
                name: "DataItems");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
