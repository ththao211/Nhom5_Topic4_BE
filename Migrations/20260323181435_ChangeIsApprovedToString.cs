using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWP_BE.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIsApprovedToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IsApproved",
                table: "TaskItemDetails",
                type: "text",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2634));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2648));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2649));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2650));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2651));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2652));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2653));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2654));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2655));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2656));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 11,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2657));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 12,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2659));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 13,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2660));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 14,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2661));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 15,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2662));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 16,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 24, 1, 14, 34, 783, DateTimeKind.Local).AddTicks(2658));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsApproved",
                table: "TaskItemDetails",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3374));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3387));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3388));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3389));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3391));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3392));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3393));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3394));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3395));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3396));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 11,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3397));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 12,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3399));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 13,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3401));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 14,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3402));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 15,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3403));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 16,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 11, 31, 47, 785, DateTimeKind.Local).AddTicks(3398));
        }
    }
}
