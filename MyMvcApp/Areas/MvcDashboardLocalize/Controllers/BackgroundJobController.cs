using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMvcApp.Areas.MvcDashboardLocalize.Models.BackgroundJob;
using MyMvcApp.Data.Localize;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardLocalize.Controllers
{
    public partial class BackgroundJobController : BaseController
    {
        #region Construction

        private readonly LocalizeDbContext context;
        private readonly ILogger logger;

        public BackgroundJobController(LocalizeDbContext context, ILogger<DomainController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        #endregion

        #region Index

        [HttpGet]
        public async Task<IActionResult> Index(IndexModel model)
        {
            var count = await context.LocalizeBackgroundJobs
                .CountAsync();
            model.MaxPage = (count + model.PageSize - 1) / model.PageSize;
            model.Items = await context.LocalizeBackgroundJobs
                .OrderBy(model.Order ?? "UtcTimeStarted DESC, Id DESC")
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToArrayAsync();

            return View("Index", model);
        }

        #endregion
    }
}
