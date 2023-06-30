using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Data.Tasks.Migrations
{
    public partial class CreateTasksSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tasks");

            migrationBuilder.CreateTable(
                name: "TaskDefinition",
                schema: "tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImplementationClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Arguments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Task",
                schema: "tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefinitionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueueName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Arguments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtcTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UtcTimeToExecute = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UtcTimeStarted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtcTimeDone = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Succeeded = table.Column<bool>(type: "bit", nullable: true),
                    Output = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Task", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Task_TaskDefinition_DefinitionId",
                        column: x => x.DefinitionId,
                        principalSchema: "tasks",
                        principalTable: "TaskDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Task_DefinitionId",
                schema: "tasks",
                table: "Task",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Task_UtcTimeToExecute",
                schema: "tasks",
                table: "Task",
                column: "UtcTimeToExecute");

            migrationBuilder.CreateIndex(
                name: "IX_Task_UtcTimeStarted",
                schema: "tasks",
                table: "Task",
                column: "UtcTimeStarted");

            migrationBuilder.CreateIndex(
                name: "IX_Task_UtcTimeDone",
                schema: "tasks",
                table: "Task",
                column: "UtcTimeDone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Task",
                schema: "tasks");

            migrationBuilder.DropTable(
                name: "TaskDefinition",
                schema: "tasks");
        }
    }
}
