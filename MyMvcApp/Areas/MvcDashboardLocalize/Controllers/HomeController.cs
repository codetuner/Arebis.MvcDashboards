using Arebis.Core.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMvcApp.Areas.MvcDashboardLocalize.Models.Home;
using MyMvcApp.Data.Localize;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardLocalize.Controllers
{
    public class HomeController : BaseController
    {
        private static bool migrationsComplete = false;

        #region Construction

        private readonly LocalizeDbContext context;
        private readonly ILogger<HomeController> logger;

        public HomeController(LocalizeDbContext context, ILogger<HomeController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Index([FromServices] ILocalizationResourceProvider? resourceProvider = null)
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

            // Check whether a resource provider is installed:
            model.HasResourceProvider = (resourceProvider != null);

            // Return index view:
            return View(model);
        }

        [HttpGet]
        public IActionResult GetStarted()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ReloadFromSource([FromServices] ILocalizationResourceProvider resourceProvider)
        {
            if (!User.IsAdministrator())
            {
                return Forbid();
            }

            resourceProvider.Refresh();

            return ForwardToAction("Index", target: "_self");
        }

        [HttpGet]
        public IActionResult RunMigrations()
        {
            context.Database.Migrate();

            return Forward(Url.Action("Index")!);
        }
    }
}
