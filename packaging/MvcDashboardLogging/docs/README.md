## Introduction

This component contains an ASP.NET Core MVC 6 item template consisting of:

- A DbContext schema for holding request log information in database,
- Request logging middleware to write failed request information to database,
- A dashboard to show and manage request log information.

## Installation

To install the project item template (to be done once per developer machine):

    dotnet new --install Arebis.MvcDashboardLogging

To add the package to an ASP.NET Core MVC 6 project, from the project folder:

    dotnet new MvcDashboardLogging

Or from the the solution folder:

    dotnet new MvcDashboardLogging -n <WebProjectName>

## Setup

Once the package added to your project, add following service registrations in Program.cs or Startup.cs:

    builder.Services.AddDbContext<MyMvcApp.Data.Logging.LoggingDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
    
    builder.Services.AddScoped<MyMvcApp.Logging.RequestLogger>();

Next, register the request logging middleware with the following command:

    app.UseArebisRequestLog()
        .LogSlowRequests()
        .LogExceptions()
        .LogNotFounds();

(Where "MyMvcApp" is the project name/namespace of your ASP.NET MVC App.)

Also make sure you have a route registered to handle ASP.NET MVC Areas:

    app.MapControllerRoute(
        name: "area",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

This route registration must be added _before_ the default route registration.

Finally, start your ASP.NET MVC application and navigate to **/MvcDashboardLogging**. Run the database migrations if requested. On the **Logs** tab you can now find logs for all failed requests.
