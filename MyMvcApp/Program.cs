using Arebis.Core.AspNet.Mvc.Localization;
using Arebis.Core.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MyMvcApp.Data;
using MyMvcApp.Localize;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


#region Localization

builder.Services.AddDbContext<MyMvcApp.Data.Localize.LocalizeDbContext>(/*contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton, */optionsAction: options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddLocalizationFromSource(builder.Configuration, options => {
    //options.CacheFileName = "LocalizationCache.json";
    options.AllowLocalizeFormat = true;
    options.Domains = new string[] { "Base", "Asp.NET", "Website" };
});

builder.Services.AddModelBindingLocalizationFromSource();

// Install optional global filters, or add them to individual controllers or actions:
builder.Services.AddControllers(config =>
{
    config.Filters.Add<ModelStateLocalizationFilter>();
    config.Filters.Add<ControllerResultLocalizationFilter>();
});

builder.Services.AddTransient<ILocalizationSource, DbContextLocalizationSource>();

//builder.Services.AddTransient<ITranslationService, DeepLTranslationService>();
//builder.Services.AddTransient<ITranslationService, GoogleTranslationService>();
builder.Services.AddTransient<ITranslationService, BingTranslationService>();

#endregion

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews()
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

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
    pattern: "{culture:regex(^[a-z][a-z](\\-[A-Z][A-Z])?$)=en}/{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{culture:regex(^[a-z][a-z](\\-[A-Z][A-Z])?$)=en}/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
