## Introduction

This component contains an ASP.NET Core MVC 8 item template consisting of:

- A DbContext schema for holding scheduled task information in database,
- A hosted (background) service to execute scheduled tasks,
- A dashboard to manage scheduled tasks.

## Installation

To install the project item template (to be done once per developer machine):

    dotnet new install Arebis.MvcDashboardTasks

To add the package to an ASP.NET Core MVC 8 project, from the project folder:

    dotnet new MvcDashboardTasks

Or from the the solution folder:

    dotnet new MvcDashboardTasks -n <WebProjectName>

## Setup

Once the package added to your project, add following service registrations in Program.cs or Startup.cs:

    builder.Services.AddDbContext<MyMvcApp.Data.Tasks.ScheduledTasksDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
    
    builder.Services.AddHostedService<MyMvcApp.Tasks.TaskScheduler>();

(Where "MyMvcApp" is the project name/namespace of your ASP.NET MVC App.)

Also make sure you have a route registered to handle ASP.NET MVC Areas:

    app.MapControllerRoute(
        name: "area",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

This route registration must be added _before_ the default route registration.

Next, apply database migrations from the Package Manager Console with:

    Update-Database -context ScheduledTasksDbContext

Or on the first run, execute the migrations from the Tasks dashboard.

## First run

Make sure you have a user in one of the following roles:
- **Administrator** or **TasksAdministrator** : will  be able to create and manage task definitions as well as tasks
- **TasksWriter** : will be able to create and manage tasks (but not task definitions)
- **TasksReader** : will have read-only access

Start your ASP.NET MVC application, log in as an administrator (to be able to create task definitions) and navigate to **/MvcDashboardTasks** to access the dashboard.
If you haven't run database migrations yet, the dasboard will offer you to do so now.

Press the **Get Started** button on the dashboard home page for further information.

