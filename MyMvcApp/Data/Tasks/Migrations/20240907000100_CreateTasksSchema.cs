using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Data.Tasks.Migrations
{
    /// <inheritdoc />
    public partial class CreateTasksSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tasks");

            migrationBuilder.CreateTable(
                name: "ScheduledTaskDefinition",
                schema: "tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImplementationClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Arguments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTaskDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTask",
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
                    table.PrimaryKey("PK_ScheduledTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledTask_ScheduledTaskDefinition_DefinitionId",
                        column: x => x.DefinitionId,
                        principalSchema: "tasks",
                        principalTable: "ScheduledTaskDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTask_DefinitionId",
                schema: "tasks",
                table: "ScheduledTask",
                column: "DefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledTask",
                schema: "tasks");

            migrationBuilder.DropTable(
                name: "ScheduledTaskDefinition",
                schema: "tasks");
        }
    }
}
