using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWP_BE.Migrations
{
    /// <inheritdoc />
    public partial class UdateRules2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(4981));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(4995));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(4996));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(4997));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(4998));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(4999));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(5000));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(5001));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(5002));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(5003));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 11,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(5004));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 12,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(5005));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 13,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(5006));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 14,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(5007));

            migrationBuilder.InsertData(
                table: "ReputationRules",
                columns: new[] { "RuleID", "Category", "Description", "IsActive", "RuleName", "UpdatedAt", "Value" },
                values: new object[] { 15, "Reward", "Thưởng Reviewer duyệt 5 task liên tiếp không bị khiếu nại sai", true, "Reward_Reviewer_Perfect_Streak", new DateTime(2026, 3, 12, 3, 12, 24, 124, DateTimeKind.Local).AddTicks(5008), 10 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 15);

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

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 12,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9118));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 13,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9118));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 14,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 12, 3, 7, 30, 226, DateTimeKind.Local).AddTicks(9119));
        }
    }
}
