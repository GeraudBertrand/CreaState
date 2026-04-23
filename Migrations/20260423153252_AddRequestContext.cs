using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreaState.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Context",
                table: "Requetes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Context",
                table: "Requetes");
        }
    }
}
