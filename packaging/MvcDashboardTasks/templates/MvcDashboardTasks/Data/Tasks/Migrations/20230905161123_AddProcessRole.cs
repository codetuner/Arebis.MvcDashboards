using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Data.Tasks.Migrations
{
    public partial class AddProcessRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessRole",
                schema: "tasks",
                table: "TaskDefinition",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessRole",
                schema: "tasks",
                table: "TaskDefinition");
        }
    }
}
