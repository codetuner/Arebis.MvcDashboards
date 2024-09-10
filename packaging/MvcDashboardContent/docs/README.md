## Introduction

This component contains an ASP.NET Core MVC 8 item template consisting of:

- A DbContext schema for holding content information in database,
- A dashboard to manage content.

## Installation

To install the project item template (to be done once per developer machine):

    dotnet new install Arebis.MvcDashboardContent

To add the package to an ASP.NET Core MVC 8 project, from the project folder:

    dotnet new MvcDashboardContent

Or from the the solution folder:

    dotnet new MvcDashboardContent -n <WebProjectName>

## Setup

Once the package added to your project, add following service registrations in Program.cs or Startup.cs:

    builder.Services.AddDbContext<MyMvcApp.Data.Content.ContentDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));

(Where "MyMvcApp" is the project name/namespace of your ASP.NET MVC App.)

Also make sure you have a route registered to handle ASP.NET MVC Areas:

    app.MapControllerRoute(
        name: "area",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

This route registration must be added _before_ the default route registration.

_After_ the registration of all routes (after all calls to MapControllerRoute or MapRazorPages),
add a registration to resolve routes by the content manager:

    app.MapControllerRoute(
        name: "content",
        pattern: "{**path}",
        defaults: new { controller = "Content", action = "Render" }
    );

<div style="border: solid 3px #c8c8c8; border-radius: 8px; padding: 8px 8px 0px 8px; margin-bottom: 12px; background-color: #f0f0f0;">

_Note: You may add additional route sections **before** the {**path} section, such as for instance a {culture} section used for route based localization:_

    app.MapControllerRoute(
        name: "content",
        pattern: "{culture}/{**path}",
        defaults: new { controller = "Content", action = "Render" }
    );
</div>

Next, add a reference to the **Arebis.Core.Services.Interfaces** nuget package.

If you want support for translations, then also add the **Arebis.Core.Services.Translation** package. This package provides translation services through the Bing, DeepL and Google Translate API's. See https://www.nuget.org/packages/Arebis.Core.Services.Translation for further setup and configuration instructions.

Finally, start your ASP.NET MVC application and navigate to **/MvcDashboardContent**. Run the database migrations if requested.

Press the **Get Started** button on the dashboard home page for further information.

