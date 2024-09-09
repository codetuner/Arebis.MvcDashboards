using Microsoft.EntityFrameworkCore.Migrations;
using MyMvcApp.Tasks;

#nullable disable

namespace MyMvcApp.Data.Tasks.Migrations
{
    /// <inheritdoc />
    public partial class SeedTasksSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("ScheduledTaskDefinition",
                new string[] { "Id", "Name", "ImplementationClass", "Arguments", "IsActive" },
                new object[] { 1, "Daily Maintenance", typeof(TasksFlushTask).FullName, "Recurrence=1\r\nSuccessRetentionDays=30\r\nFailureRetentionDays=90", false },
                "tasks");

            migrationBuilder.InsertData("ScheduledTaskDefinition",
                new string[] { "Id", "Name", "ImplementationClass", "Arguments", "IsActive" },
                new object[] { 2, "Sample Counting Task", typeof(SampleCountingTask).FullName, (string)null, true },
                "tasks");

            //migrationBuilder.InsertData("ScheduledTask",
            //    new string[] { "Id", "DefinitionId", "Arguments", "UtcTimeCreated", "UtcTimeToExecute" },
            //    new object[] { 1, 2, "Count=10", DateTime.UtcNow, DateTime.UtcNow.AddHours(4) },
            //    "tasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("ScheduledTaskDefinition", "Id", new object[] { 1, 2 }, "tasks");
            
            //migrationBuilder.DeleteData("ScheduledTask", "Id", new object[] { 1 }, "tasks");
        }
    }
}
