## Introduction

This component contains an ASP.NET Core MVC 6 item template consisting of:

- A DbContext schema for holding scheduled task information in database,
- A hosted (background) service to execute scheduled tasks,
- A dashboard to manage scheduled tasks.

## Installation

To install the project item template (to be done once per developer machine):

    dotnet new --install Arebis.MvcDashboardTasks

To add the package to an ASP.NET Core MVC 6 project, from the project folder:

    dotnet new MvcDashboardTasks

Or from the the solution folder:

    dotnet new MvcDashboardTasks -n <WebProjectName>

## Setup

Once the package added to your project, add following service registrations in Program.cs or Startup.cs:

    builder.Services.AddDbContext<MyMvcApp.Data.Tasks.TasksDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
    
    builder.Services.AddHostedService<MyMvcApp.Tasks.TaskScheduler>();

(Where "MyMvcApp" is the project name/namespace of your ASP.NET MVC App.)

Also make sure you have a route registered to handle ASP.NET MVC Areas:

    app.MapControllerRoute(
        name: "area",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

This route registration must be added _before_ the default route registration.

Finally, start your ASP.NET MVC application and navigate to **/MvcDashboardTasks**. Run the database migrations if requested.

Press the **Get Started** button on the dashboard home page for further information.

