using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Areas.MvcDashboardLogging.Models.Rules;
using MyMvcApp.Data.Logging;
using MyMvcApp.Logging;

namespace MyMvcApp.Areas.MvcDashboardLogging.Controllers
{
    [Authorize(Roles = "Administrator,LoggingAdministrator")]
    public class RulesController : BaseController
    {
        #region Construction

        private readonly LoggingDbContext context;
        private readonly ILogger logger;

        public RulesController(LoggingDbContext context, ILogger<RulesController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        #endregion

        #region Index

        [HttpGet]
        public IActionResult Index(IndexModel model)
        {
            // Retrieve data:
            var query = context.LogActionRules
                .AsQueryable()
                .Where(d => model.ApplicationFilter == null || d.ApplicationName == model.ApplicationFilter)
                .Where(d => model.AspectFilter == null || d.AspectName == model.AspectFilter)
                .Where(d => model.ActionFilter == null || d.Action == model.ActionFilter)
                .Where(d => model.ActiveFilter == false || d.IsActive == true);
            if (!String.IsNullOrWhiteSpace(model.Query))
                query = query
                    .Where(d => d.Url!.Contains(model.Query) || d.Type!.Contains(model.Query) || d.ActionData!.Contains(model.Query));

            // Build model:
            var count = query
                .Count();
            model.MaxPage = (count + model.PageSize - 1) / model.PageSize;
            model.Items = query
                .OrderBy(model.Order ?? "Url")
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToArray();

            // Render view:
            return View("Index", model);
        }

        #endregion

        #region Edit

        [HttpGet]
        public IActionResult New(int? cloneOfId)
        {
            var original = context.LogActionRules.Find(cloneOfId ?? 0);
            var model = (original == null)
                ? new EditModel
                {
                    Item = new LogActionRule()
                    {
                        Action = Logging.LogAction.DoNotLog,
                        IsActive = true,
                    }
                }
                : new EditModel()
                {
                    Item = new LogActionRule()
                    {
                        Action = original.Action,
                        ApplicationName = original.ApplicationName,
                        ActionData = original.ActionData,
                        AspectName = original.AspectName,
                        IsActive=original.IsActive,
                        Host = original.Host,
                        Method = original.Method,
                        StatusCode  = original.StatusCode,
                        Type = original.Type,
                        Url = original.Url,
                    },
                    HasChanges = true
                };

            return EditView(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var item = context.LogActionRules.Find(id);
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

        [HttpPost]
        public IActionResult Submit([Bind(Prefix = "id")] int _, EditModel model)
        {
            ModelState.Clear();
            model.HasChanges = true;
            return EditView(model);
        }

        public async Task<IActionResult> Save(int id, EditModel model, bool apply = false)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Perform save:
                    context.Update(model.Item);
                    await context.SaveChangesAsync();

                    // Flush cached rules:
                    RequestDoNotLogRuleFilter.FlushDoNotLogRulesCache();

                    // Return:
                    if (!apply)
                    {
                        return Back(false);
                    }
                    else
                    {
                        ModelState.Clear();
                        model.HasChanges = false;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error saving rule {ObjectId}", id);
                    ModelState.AddModelError("", "An unexpected error occured.");
                    ViewBag.Exception = ex;
                }
            }
            else
            {
                SetToastrMessage("error", "Failed to save the rule.<br/>See validation messages for more information.");
            }

            //Response.Headers.Add("X-Sircl-History-Replace", Url.Action("Edit", new { id = model.Item!.Id }));
            return EditView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, EditModel model)
        {
            try
            {
                var item = await context.LogActionRules.FindAsync(id);
                if (item == null)
                {
                    this.SetToastrMessage("error", "Rule not found.");
                }
                else
                {
                    context.Remove(item);
                    await context.SaveChangesAsync();
                    this.SetToastrMessage("success", "Rule deleted");
                }
                return Back(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error deleting rule {ObjectId}", id);
                ModelState.AddModelError("", "An unexpected error occured.");
                ViewBag.Exception = ex;
            }

            return EditView(model);
        }

        private IActionResult EditView(EditModel model)
        {
            return View("Edit", model);
        }

        #endregion
    }
}
