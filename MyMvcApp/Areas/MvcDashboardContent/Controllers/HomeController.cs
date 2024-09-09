using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Areas.MvcDashboardContent.Models.Home;
using MyMvcApp.Data.Content;

namespace MyMvcApp.Areas.MvcDashboardContent.Controllers
{
    [Authorize(Roles = "Administrator,ContentAdministrator,ContentEditor,ContentAuthor")]
    public class HomeController : BaseController
    {
        private static bool migrationsComplete = false;

        #region Construction

        private readonly ContentDbContext context;
        private readonly ILogger logger;

        public HomeController(ContentDbContext context, ILogger<HomeController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new IndexModel();

            // Check if there are pending migrations:
            if (!migrationsComplete)
            {
                var migrations = await context.Database.GetPendingMigrationsAsync();
                if (migrations != null && migrations.Any())
                {
                    model.HasPendingMigrations = true;
                }
                else
                {
                    // Skip checking next time:
                    migrationsComplete = true;
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult GetStarted()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RunMigrations()
        {
            context.Database.Migrate();

            return Forward(Url.Action("Index")!);
        }
    }
}
