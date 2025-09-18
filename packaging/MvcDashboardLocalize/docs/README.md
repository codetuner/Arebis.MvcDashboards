## Introduction

This component contains an ASP.NET Core MVC 8 item template consisting of an area named "MvcDashboardLocalize" implementing a dashboard to manage application localization on database, as well as the database definition and classes needed to localize the application.

To get started, follow these videos:

>    [MvcDashboardLocalize intro - part 1](https://www.youtube.com/watch?v=rzU8rQwJzzU)

>    [MvcDashboardLocalize intro - part 2](https://www.youtube.com/watch?v=IL9YC28_mr8)

## Installation

To install the project item template (to be done once per developer machine):

    dotnet new install Arebis.MvcDashboardLocalize

To add the dashboard to an ASP.NET Core MVC 8+ project, from the project folder:

    dotnet new MvcDashboardLocalize --force

Or from the the solution folder:

    dotnet new MvcDashboardLocalize -n <WebProjectName> --force

(The --force options will install the required package dependencies)

Then open the "**Areas\MvcDashboardLocalize\ReadMe-MvcDashboardLocalize.html**" file in a browser for further instructions.

## Setup

Once the package added to your project, add following service registrations in Program.cs or Startup.cs:

    builder.Services
        .AddDbContext<MyMvcApp.Data.Localize.LocalizeDbContext>(
        optionsAction: options => options.UseSqlServer(
          builder.Configuration.GetConnectionString("DefaultConnection")));

(Where "MyMvcApp" is the project name/namespace of your ASP.NET MVC App.)

Also make sure you have a route registered to handle ASP.NET MVC Areas:

    app.MapControllerRoute(
        name: "area",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

This route registration must be added _before_ the default route registration.

## First run

You can navigate to the dashboard and apply database migrations from there, or you can apply apply database migrations from the Package Manager Console with:

    Update-Database -context LocalizeDbContext

Finally, start your ASP.NET MVC application and navigate to **/MvcDashboardLocalize** to access the dashboard.
If you haven't run database migrations yet, the dasboard will offer you to do so now.

To make your application localizable, read the article published on CodeProject:
> [Localizing ASP.NET Core MVC Applications from Database](https://web.archive.org/web/20241113103935/https://www.codeproject.com/Articles/5348357/Localizing-ASP-NET-Core-MVC-applications-from-data)
