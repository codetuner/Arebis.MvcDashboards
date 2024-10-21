using Arebis.Core.AspNet.Mvc.Localization;
using Arebis.Core.Localization;
using Arebis.Core.Services.Interfaces;
using Arebis.Core.Services.Translation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyMvcApp.Data;
using MyMvcApp.Data.Content;
using MyMvcApp.Localize;
using MyMvcApp.Logging;
using System.Collections.Generic;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

#region Content

builder.Services.AddDbContext<ContentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

#endregion

#region Localization

builder.Services.AddDbContext<MyMvcApp.Data.Localize.LocalizeDbContext>(/*contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton, */optionsAction: options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddLocalizationFromSource(builder.Configuration, options => {
    options.AllowLocalizeFormat = false;
    options.CacheFileName = "LocalizationCache.json";
    options.Domains = new string[] { "Base", "Asp.NET", "Test", "Website" };
//    options.Domains = new string[] { "Test", "TestApp" };
    //options.RouteDataStringKey = "culture";
    //options.UIRouteDataStringKey = "uiculture";
    options.UseOnlyReviewedLocalizationValues = false;
});

builder.Services.AddModelBindingLocalizationFromSource();

// Install optional global filters, or add them to individual controllers or actions:
builder.Services.AddControllers(config =>
{
    config.Filters.Add<ModelStateLocalizationFilter>();
});

builder.Services.AddTransient<ILocalizationSource, DbContextLocalizationSource>();

//builder.Services.AddTransient<ITranslationService, DeepLTranslationService>();
builder.Services.AddTransient<ITranslationService, GoogleTranslationService>();
//builder.Services.AddTransient<ITranslationService, BingTranslationService>();

#endregion

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews()
//    .AddDataAnnotationsLocalization()
#region Localization
    .AddDataAnnotationsLocalizationFromSource()
#endregion
    ;

#region Request Localization

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    List<CultureInfo> supportedCultures = new List<CultureInfo>
        {
            new CultureInfo("en"),
            new CultureInfo("nl"),
            new CultureInfo("fr"),
            new CultureInfo("de"),
        };

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
    options.RequestCultureProviders.Add(new RouteDataRequestCultureProvider());
    options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
    //options.RequestCultureProviders.Add(new CustomRequestCultureProvider(async context =>
    //{
    //    //Write your code here
    //    return new ProviderCultureResult("nl-BE");
    //}));
});

#endregion

#region Tasks

builder.Services.AddDbContext<MyMvcApp.Data.Tasks.ScheduledTasksDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddHostedService<MyMvcApp.Tasks.TaskScheduler>();

#endregion

#region Logging

builder.Services.AddDbContext<MyMvcApp.Data.Logging.LoggingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddScoped<MyMvcApp.Logging.RequestLogger>();

#endregion

#region Output caching

// Is not set by default; need to call AddOuptutCache() on services, and UseOutputCache on app (after calling UseRouting):
builder.Services.AddOutputCache();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

#region Logging

app.UseArebisRequestLog()
    .ApplyDoNotLogRule()
    .LogSlowRequests()
    .LogExceptions()
    .LogNotFounds();

#endregion

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

#region Output caching

// Is not set by default; need to call AddOuptutCache() on services, and UseOutputCache on app (after calling UseRouting):
app.UseOutputCache();

#endregion

app.UseAuthentication();
app.UseAuthorization();

#region Localization

app.UseLocalizationFromSource();

#endregion

#region Request Localization

var rloptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(rloptions!.Value);

#endregion

app.MapControllerRoute(
    name: "area",
    pattern: "{culture:regex(^[a-z][a-z](\\-[A-Z][A-Z]){{0,1}}$)=en}/{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{culture:regex(^[a-z][a-z](\\-[A-Z][A-Z]){{0,1}}$)=en}/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

#region Content

app.MapControllerRoute(
    name: "content",
    //pattern: "{**path}",
    pattern: "{culture:regex(^[a-z][a-z](\\-[A-Z][A-Z]){{0,1}}$)=en}/{**path}",
    defaults: new { controller = "Content", action = "Render" }
);

#endregion

app.MapFallbackToFile("404.html");

app.Run();
