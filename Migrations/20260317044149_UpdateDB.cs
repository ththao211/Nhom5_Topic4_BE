using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWP_BE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- THÊM 2 CỘT BỊ THIẾU VÀO BẢNG ReviewerStats ---
            migrationBuilder.AddColumn<int>(
                name: "FirstTryApprovedTasks",
                table: "ReviewerStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentPerfectStreak",
                table: "ReviewerStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            // ----------------------------------------------------

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2554));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2570));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2572));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2573));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2574));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2576));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2577));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2578));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2579));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2580));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 11,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 41, 49, 474, DateTimeKind.Local).AddTicks(2582));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- XÓA 2 CỘT NẾU ROLLBACK LẠI ---
            migrationBuilder.DropColumn(
                name: "FirstTryApprovedTasks",
                table: "ReviewerStats");

            migrationBuilder.DropColumn(
                name: "CurrentPerfectStreak",
                table: "ReviewerStats");
            // -----------------------------------

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9591));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9602));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9603));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9604));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9605));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9606));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9607));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9608));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9609));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9610));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 11,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 17, 11, 30, 50, 628, DateTimeKind.Local).AddTicks(9611));
        }
    }
}