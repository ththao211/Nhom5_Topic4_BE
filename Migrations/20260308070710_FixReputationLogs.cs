using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWP_BE.Migrations
{
    public partial class FixReputationLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ADD NEW COLUMNS
            migrationBuilder.AddColumn<int>(
                name: "OldScore",
                table: "ReputationLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NewScore",
                table: "ReputationLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RuleID",
                table: "ReputationLogs",
                type: "int",
                nullable: true);

            // CREATE INDEX
            migrationBuilder.CreateIndex(
                name: "IX_ReputationLogs_RuleID",
                table: "ReputationLogs",
                column: "RuleID");

            // ADD FOREIGN KEY
            migrationBuilder.AddForeignKey(
                name: "FK_ReputationLogs_ReputationRules_RuleID",
                table: "ReputationLogs",
                column: "RuleID",
                principalTable: "ReputationRules",
                principalColumn: "RuleID",
                onDelete: ReferentialAction.Restrict);

            // UPDATE SEED DATA
            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3872));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3883));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3885));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3886));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3887));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3888));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3889));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3890));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3891));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3892));

            migrationBuilder.UpdateData(
                table: "ReputationRules",
                keyColumn: "RuleID",
                keyValue: 11,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 8, 14, 7, 10, 223, DateTimeKind.Local).AddTicks(3893));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReputationLogs_ReputationRules_RuleID",
                table: "ReputationLogs");

            migrationBuilder.DropIndex(
                name: "IX_ReputationLogs_RuleID",
                table: "ReputationLogs");

            migrationBuilder.DropColumn(
                name: "OldScore",
                table: "ReputationLogs");

            migrationBuilder.DropColumn(
                name: "NewScore",
                table: "ReputationLogs");

            migrationBuilder.DropColumn(
                name: "RuleID",
                table: "ReputationLogs");
        }
    }
}