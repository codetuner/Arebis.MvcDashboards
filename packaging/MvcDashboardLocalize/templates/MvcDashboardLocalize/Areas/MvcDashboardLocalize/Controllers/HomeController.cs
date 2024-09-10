using Arebis.Core.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MyMvcApp.Areas.MvcDashboardLocalize.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetStarted()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ReloadFromSource([FromServices] ILocalizationResourceProvider resourceProvider)
        {
            resourceProvider.Refresh();

            return ForwardToAction("Index", target: "_self");
        }
    }
}
