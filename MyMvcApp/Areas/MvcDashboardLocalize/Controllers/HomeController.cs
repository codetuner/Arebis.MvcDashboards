using Arebis.Core.AspNet.Mvc.Localization;
using Arebis.Core.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Data.Localize;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardLocalize.Controllers
{
    [Authorize(Roles = "Administrator,LocalizeAdministrator,LocalizeTranslator")]
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
        [Authorize(Roles = "Administrator,LocalizeAdministrator")]
        public IActionResult ReloadFromSource([FromServices] ILocalizationResourceProvider resourceProvider)
        {
            resourceProvider.Refresh();

            return ForwardToAction("Index", target: "_self");
        }
    }
}
