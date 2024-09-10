
To create new migrations:

PM> Add-Migration CreateTasksSchema -Context ScheduledTasksDbContext -OutputDir Data\Tasks\Migrations

To apply migrations:

PM> Update-Database -Context ScheduledTasksDbContext

