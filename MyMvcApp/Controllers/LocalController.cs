using Arebis.Core.AspNet.Mvc.Localization;
using Arebis.Core.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using MyMvcApp.Models.Local;

namespace MyMvcApp.Controllers
{
    //[ServiceFilter(typeof(ModelStateLocalizationFilter))]
    public class LocalController : Controller
    {
        private readonly IStringLocalizer<LocalController> stringLocalizer;
        private readonly IHtmlLocalizer<LocalController> htmlLocalizer;

        public LocalController(IStringLocalizer<LocalController> stringLocalizer, IHtmlLocalizer<LocalController> htmlLocalizer)
        {
            this.stringLocalizer = stringLocalizer;
            this.htmlLocalizer = htmlLocalizer;
        }

        //[ServiceFilter(typeof(ModelStateLocalizationFilter))]
        public IActionResult Index(IndexModel? model)
        {
            model ??= new IndexModel();

            if ((model.Name?.Length ?? 0) < 5)
            {
                model.Message = stringLocalizer["This is a short name for {0}.", "strings < 5 chars"];
                model.HtmlMessage = htmlLocalizer["This is a <i>short</i> name for <b>{0}</b>.", "strings < 5 chars"];
            }

            model.Date = DateTime.Now;

            ViewBag.Date = DateTime.Now;

            return View("Index", model);
        }

        //[ServiceFilter(typeof(ModelStateLocalizationFilter))]
        public IActionResult Da(IndexModel? model)
        {
            model ??= new IndexModel();
            return View("Da", model);
        }
    }
}
