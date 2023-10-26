using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Data.Logging.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLoggingSchemaAddApplicationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationName",
                schema: "logging",
                table: "RequestLog",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationName",
                schema: "logging",
                table: "LogActionRule",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationName",
                schema: "logging",
                table: "RequestLog");

            migrationBuilder.DropColumn(
                name: "ApplicationName",
                schema: "logging",
                table: "LogActionRule");
        }
    }
}
