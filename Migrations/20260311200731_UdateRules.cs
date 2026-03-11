using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SWP_BE.Migrations
{
    /// <inheritdoc />
    public partial class UdateRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9092));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9107));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9109));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9110));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9111));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9112));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9113));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9114));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9115));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9116));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 11,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9117));

            migrationBuilder.InsertData(
                table: "ReputationRules",
                columns: new[] { "RuleID", "Category", "Description", "IsActive", "RuleName", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 12, "Penalty", "Reviewer bắt lỗi sai (Dispute lost)", true, "Penalty_Reviewer_False_Check", new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9118), -10 },
                    { 13, "Penalty", "Annotator khiếu nại sai (Dispute lost)", true, "Penalty_Annotator_False_Dispute", new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9118), -5 },
                    { 14, "Limit", "Số lần khiếu nại sai tối đa để bị khóa tài khoản", true, "Max_False_Disputes", new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9119), 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 14);

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5557));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5558));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5559));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5560));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5561));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5562));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5563));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5564));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5565));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 11,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 11, 23, 13, 58, 751, DateTimeKind.Local).AddTicks(5566));
        }
    }
}
