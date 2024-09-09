using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Data.Tasks.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTasksSchemaAddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTask_MachineNameToRunOn",
                schema: "tasks",
                table: "ScheduledTask",
                column: "MachineNameToRunOn");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTask_UtcTimeToExecute",
                schema: "tasks",
                table: "ScheduledTask",
                column: "UtcTimeToExecute");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTask_UtcTimeStarted",
                schema: "tasks",
                table: "ScheduledTask",
                column: "UtcTimeStarted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_ScheduledTask_MachineNameToRunOn", schema: "tasks", table: "ScheduledTask");

            migrationBuilder.DropIndex("IX_ScheduledTask_UtcTimeToExecute", schema: "tasks", table: "ScheduledTask");

            migrationBuilder.DropIndex("IX_ScheduledTask_UtcTimeStarted", schema: "tasks", table: "ScheduledTask");
        }
    }
}
