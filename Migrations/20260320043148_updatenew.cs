using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWP_BE.Migrations
{
    /// <inheritdoc />
    public partial class updatenew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ErrorRegion",
                table: "ReviewComments",
                newName: "EvidenceImages");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EvidenceImages",
                table: "ReviewComments",
                newName: "ErrorRegion");

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3827));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3837));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3838));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3840));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3862));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3864));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3865));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3866));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3867));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3868));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 11,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3868));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 12,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3870));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 13,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3871));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 14,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3872));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 15,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3873));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 16,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 20, 10, 39, 58, 13, DateTimeKind.Local).AddTicks(3869));
        }
    }
}
