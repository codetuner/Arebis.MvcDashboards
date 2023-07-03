using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Areas.MvcDashboardLogging.Models.Items;
using MyMvcApp.Data;
using MyMvcApp.Data.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardLogging.Controllers
{
    [Authorize(Roles = "Administrator,LoggingAdministrator")]
    public class ItemsController : BaseController
    {
        #region Construction

        private readonly LoggingDbContext context;

        public ItemsController(LoggingDbContext context)
        {
            this.context = context;
        }

        #endregion

        #region Index

        [HttpGet]
        public IActionResult Index(IndexModel model)
        {
            // Retrieve data:
            var query = context.RequestLogs.AsQueryable();
            if (!String.IsNullOrWhiteSpace(model.Query))
                query = query.Where(d => d.Message!.Contains(model.Query) || d.Url!.Contains(model.Query) || d.User!.Contains(model.Query) || d.Details!.Contains(model.Query));

            // Build model:
            var count = query
                .Count();
            model.MaxPage = (count + model.PageSize - 1) / model.PageSize;
            model.Items = query
                .OrderBy(model.Order ?? "Id DESC")
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToArray();

            // Render view:
            return View("Index", model);
        }

        #endregion

        #region Edit

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var item = context.RequestLogs.Find(id);
            if (item == null)
            {
                return new NotFoundResult();
            }
            else
            {
                var model = new EditModel { Item = item };
                return EditView(model);
            }
        }

        private IActionResult EditView(EditModel model)
        {
            return View("Edit", model);
        }

        #endregion

        #region Bookmarking

        public IActionResult AddBookmark(int id)
        {
            var log = context.RequestLogs.Find(id);
            if (log != null)
            {
                log.IsBookmarked = true;
                context.SaveChanges();
            }
            return PartialView("Bookmark", log);
        }

        public IActionResult RemoveBookmark(int id)
        {
            var log = context.RequestLogs.Find(id);
            if (log != null)
            {
                log.IsBookmarked = false;
                context.SaveChanges();
            }
            return PartialView("Bookmark", log);
        }

        #endregion
    }
}
