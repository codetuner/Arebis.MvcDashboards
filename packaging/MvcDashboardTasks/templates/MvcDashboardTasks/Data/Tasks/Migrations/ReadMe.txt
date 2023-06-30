
To create new migrations:

PM> Add-Migration CreateTasksSchema -Context TasksDbContext -OutputDir Data\Tasks\Migrations

To apply migrations:

PM> Update-Database -Context TasksDbContext

