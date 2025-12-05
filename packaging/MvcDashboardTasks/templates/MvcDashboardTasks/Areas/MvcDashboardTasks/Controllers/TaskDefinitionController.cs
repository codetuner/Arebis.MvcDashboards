using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMvcApp.Areas.MvcDashboardTasks.Models.TaskDefinition;
using MyMvcApp.Data.Tasks;
using MyMvcApp.Tasks;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardTasks.Controllers
{
    public partial class TaskDefinitionController : BaseController
    {
        #region Construction

        private readonly ScheduledTasksDbContext context;
        private readonly ILogger logger;

        public TaskDefinitionController(ScheduledTasksDbContext context, ILogger<TaskDefinitionController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        #endregion

        #region Index

        [HttpGet]
        public async Task<IActionResult> Index(IndexModel model)
        {
            var noQuery = String.IsNullOrWhiteSpace(model.Query);
            var count = await context.TaskDefinitions
                .Where(i => i.ProcessRole == model.ProcessRole || model.ProcessRole == null)
                .Where(i => noQuery || i.Name!.Contains(model.Query ?? ""))
                .CountAsync();
            model.MaxPage = (count + model.PageSize - 1) / model.PageSize;
            model.Items = await context.TaskDefinitions
                .Where(i => i.ProcessRole == model.ProcessRole || model.ProcessRole == null)
                .Where(i => noQuery || i.Name!.Contains(model.Query ?? ""))
                .OrderBy(model.Order ?? "Name ASC, Id ASC")
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToArrayAsync();

            model.ProcessRoles = (TaskController.ProcessRoles ??= await context.TaskDefinitions.Where(d => d.ProcessRole != null).Select(d => d.ProcessRole!).Distinct().OrderBy(r => r).Select(r => new SelectListItem() { Value = r, Text = r, Selected = (model.ProcessRole == r) }).ToListAsync());

            model.UserIsAdmin = this.UserIsAdministrator;

            return View("Index", model);
        }

        #endregion

        #region Edit

        [HttpGet]
        public IActionResult New()
        {
            // Check authorization:
            if (!this.UserIsAdministrator) return Forbid();

            var model = new EditModel
            {
                Item = new Data.Tasks.ScheduledTaskDefinition()
            };

            return EditView(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await context.TaskDefinitions
                .SingleOrDefaultAsync(k => k.Id == id);
            if (item == null) return new NotFoundResult();

            var model = new EditModel() { Item = item };

            return EditView(model);
        }

        [HttpPost]
        public IActionResult Submit([Bind(Prefix = "id")]int _, EditModel model)
        {
            ModelState.Clear();
            model.HasChanges = true;

            // Handle update of TaskDefinition
            return EditView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(int id, EditModel model, bool apply = false)
        {
            // Check authorization:
            if (!this.UserIsAdministrator) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(model.Item);
                    await context.SaveChangesAsync();
                    TaskController.ProcessRoles = null; // Flush cached data
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
                    logger.LogError(ex, "Unexpected error saving task definition {ObjectId}", id);
                    ModelState.AddModelError("", "An unexpected error occured.");
                    ViewBag.Exception = ex;
                }
            }
            else
            {
                SetToastrMessage("error", "Failed to save the task definition.<br/>See validation messages for more information.");
            }

            Response.Headers["X-Sircl-History-Replace"] = Url.Action("Edit", new { id = model.Item!.Id });
            return EditView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, EditModel model)
        {
            // Check authorization:
            if (!this.UserIsAdministrator) return Forbid();

            try
            {
                var item = await context.TaskDefinitions.FindAsync(id);
                if (item == null)
                {
                    this.SetToastrMessage("error", "Task definition not found.");
                }
                else if (context.Tasks.Any(t => t.DefinitionId == id && t.UtcTimeStarted != null))
                {
                    this.SetToastrMessage("error", "Cannot delete definitions when tasks have already run or started.");
                }
                else
                {
                    context.Remove(item);
                    await context.SaveChangesAsync();
                    TaskController.ProcessRoles = null; // Flush cached data

                    this.SetToastrMessage("success", "Task definition deleted");
                }
                return Back(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error deleting task definition {ObjectId}", id);
                ModelState.AddModelError("", "An unexpected error occured.");
                ViewBag.Exception = ex;
            }

            return EditView(model);
        }

        private IActionResult EditView(EditModel model)
        {
            // Retrieve candidate implementation types:
            model.ImplementationCandidateNames = Assembly.GetEntryAssembly()!.DefinedTypes
                .Where(t => t.IsPublic && !t.IsAbstract && typeof(IScheduledTaskImplementation).IsAssignableFrom(t))
                .Select(t => t.FullName!)
                .OrderBy(n => n)
                .ToList();

            // Retrieve candidate process role names:
            model.ProcessRoleCandidateNames = context.TaskDefinitions
                .Where(d => d.ProcessRole != null)
                .Select(d => d.ProcessRole!)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            // Retrieve authorization:
            model.UserIsAdmin = this.UserIsAdministrator;

            // Return the view:
            return View("Edit", model);
        }

        #endregion
    }
}
