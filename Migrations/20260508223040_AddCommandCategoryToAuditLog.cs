using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StadiumSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCommandCategoryToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommandCategory",
                table: "AuditLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommandCategory",
                table: "AuditLogs");
        }
    }
}
