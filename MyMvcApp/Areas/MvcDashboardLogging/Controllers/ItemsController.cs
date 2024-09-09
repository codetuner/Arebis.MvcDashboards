using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MyMvcApp.Areas.MvcDashboardLogging.Models.Items;
using MyMvcApp.Data.Logging;

namespace MyMvcApp.Areas.MvcDashboardLogging.Controllers
{
    [Authorize(Roles = "Administrator,LoggingAdministrator")]
    public class ItemsController : BaseController
    {
        #region Construction

        private readonly LoggingDbContext context;

        private readonly IConfiguration configuration;

        public ItemsController(LoggingDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        #endregion

        #region Index

        [HttpGet]
        public IActionResult Index(IndexModel model)
        {
            // Retrieve data:
            var query = context.RequestLogs
                .AsQueryable()
                .Where(d => model.ApplicationFilter == null || d.ApplicationName == model.ApplicationFilter)
                .Where(d => model.AspectFilter == null || d.AspectName == model.AspectFilter)
                .Where(d => model.BookmarkedFilter == false || d.IsBookmarked == true);
            if (!String.IsNullOrWhiteSpace(model.Query))
                query = query
                    .Where(d => d.TraceIdentifier == model.Query
                        || d.Message!.Contains(model.Query)
                        || d.Url!.Contains(model.Query)
                        || d.User!.Contains(model.Query)
                        || d.Details!.Contains(model.Query));

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

        [OutputCache(Duration = 300)]
        public IActionResult ApplicationNameOptions(string? selected)
        {
            var model = new ApplicationNameOptionsModel();
            model.Options = context.RequestLogs.Where(l => l.ApplicationName != null).Select(l => l.ApplicationName).Distinct().ToList();
            model.SelectedOption = selected;

            return View(model);
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

        #region Logging rules

        public IActionResult CreateIgnoreRule(int id)
        {
            var log = context.RequestLogs.Find(id);
            if (log != null)
            {
                var matchingRules = context.LogActionRules
                    .Where(r => r.Action == Logging.LogAction.DoNotLog && r.Url == log.Url && r.StatusCode == log.StatusCode && r.AspectName == log.AspectName && r.Method == log.Method && r.Type == log.Type)
                    .ToList();

                if (matchingRules.Any(r => r.IsActive == true))
                {
                    this.SetToastrMessage("info", "There is already a matching rule. The server may need to be restarted for the rule to be applied.");
                }
                else if (matchingRules.Any(r => r.IsActive == false))
                {
                    foreach(var rule in matchingRules)
                    {
                        rule.IsActive = true;
                    }
                    this.SetToastrMessage("success", "A rule existed and has now been activated.");
                }
                else
                {
                    context.Add(new LogActionRule()
                    {
                        Action = Logging.LogAction.DoNotLog,
                        Method = log.Method,
                        StatusCode = log.StatusCode,
                        AspectName = log.AspectName,
                        Type = log.Type,
                        Url = log.Url,
                        IsActive = true
                    });
                    this.SetToastrMessage("success", "A rule has been created to ignore similar logs in the future.");
                }

                context.SaveChanges();
            }
            else
            {
                this.SetToastrMessage("error", "Failed to create rule: missing template log.");
            }

            return NoContent();
        }

        #endregion
    }
}
