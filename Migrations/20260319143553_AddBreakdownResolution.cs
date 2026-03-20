using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreaState.Migrations
{
    /// <inheritdoc />
    public partial class AddBreakdownResolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResolved",
                table: "MaintenanceRecords",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "MaintenanceRecords",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResolvedByMemberId",
                table: "MaintenanceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_ResolvedByMemberId",
                table: "MaintenanceRecords",
                column: "ResolvedByMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_Users_ResolvedByMemberId",
                table: "MaintenanceRecords",
                column: "ResolvedByMemberId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Users_ResolvedByMemberId",
                table: "MaintenanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_ResolvedByMemberId",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "IsResolved",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "ResolvedByMemberId",
                table: "MaintenanceRecords");
        }
    }
}
