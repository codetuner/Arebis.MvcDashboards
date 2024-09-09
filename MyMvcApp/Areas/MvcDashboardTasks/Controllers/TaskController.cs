using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMvcApp.Areas.MvcDashboardTasks.Models.Task;
using MyMvcApp.Data.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardTasks.Controllers
{
    public class TaskController : BaseController
    {
        internal static List<SelectListItem>? ProcessRoles = null;

        #region Construction

        private readonly ScheduledTasksDbContext context;
        private readonly ILogger logger;

        public TaskController(ScheduledTasksDbContext context, ILogger<TaskController> logger)
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
            var count = await context.Tasks
                .Where(i => i.DefinitionId == model.DefinitionId || model.DefinitionId == null)
                .Where(i => i.Definition.ProcessRole == model.ProcessRole || model.ProcessRole == null)
                .Where(i => noQuery || i.Name!.Contains(model.Query ?? "") || i.Definition.Name!.Contains(model.Query ?? "") || i.QueueName == model.Query || i.MachineNameToRunOn == model.Query || i.MachineNameRanOn == model.Query)
                .Where(i => model.StatusFilter != "Queued" || (i.UtcTimeDone == null) && (i.Definition.IsActive || i.UtcTimeStarted != null))
                .Where(i => model.StatusFilter != "Running" || (i.UtcTimeDone == null && i.UtcTimeStarted != null))
                .Where(i => model.StatusFilter != "Succeeded" || i.Succeeded == true)
                .Where(i => model.StatusFilter != "Failed" || i.Succeeded == false)
                .CountAsync();
            model.MaxPage = (count + model.PageSize - 1) / model.PageSize;
            model.Items = await context.Tasks
                .Include(i => i.Definition)
                .Where(i => i.DefinitionId == model.DefinitionId || model.DefinitionId == null)
                .Where(i => i.Definition.ProcessRole == model.ProcessRole || model.ProcessRole == null)
                .Where(i => noQuery || i.Name!.Contains(model.Query ?? "") || i.Definition.Name!.Contains(model.Query ?? "") || i.QueueName == model.Query || i.MachineNameToRunOn == model.Query || i.MachineNameRanOn == model.Query)
                .Where(i => model.StatusFilter != "Queued" || (i.UtcTimeDone == null) && (i.Definition.IsActive || i.UtcTimeStarted != null))
                .Where(i => model.StatusFilter != "Running" || (i.UtcTimeDone == null && i.UtcTimeStarted != null))
                .Where(i => model.StatusFilter != "Succeeded" || i.Succeeded == true)
                .Where(i => model.StatusFilter != "Failed" || i.Succeeded == false)
                .OrderBy(model.Order ?? "Id DESC")
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToArrayAsync();
            model.Definitions = await context.TaskDefinitions
                .OrderBy(d => d.Name).ThenBy(d => d.Id)
                .Select(d => new SelectListItem() { Value = d.Id.ToString(), Text = d.Name, Selected = (model.DefinitionId == d.Id) })
                .ToListAsync();
            model.ProcessRoles = (TaskController.ProcessRoles ??= await context.TaskDefinitions.Where(d => d.ProcessRole != null).Select(d => d.ProcessRole!).Distinct().OrderBy(r => r).Select(r => new SelectListItem() { Value = r, Text = r, Selected = (model.ProcessRole == r) }).ToListAsync());

            return View("Index", model);
        }

        public async Task<IActionResult> IndexDelete(IndexModel model)
        {
            var deletedCount = 0;
            var skippedCount = 0;
            if (model.Selection != null)
            {
                foreach (var id in model.Selection)
                {
                    var task = context.Tasks.Find(id);
                    if (task != null && task.UtcTimeStarted == null)
                    {
                        context.Tasks.Remove(task);
                        deletedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                await context.SaveChangesAsync();
            }
            
            if (skippedCount > 0)
            {
                this.SetToastrMessage("warning", $"{deletedCount} task(s) deleted. Some tasks were already running or completed and could not be deleted.");
            }
            else if (deletedCount> 0)
            {
                this.SetToastrMessage("success", $"{deletedCount} task(s) deleted.");
            }


            model.Selection = null;
            model.SelectionMaster = false;
            ModelState.Clear();
            return await Index(model);
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> New(int? definitionId, int? cloneOfId)
        {
            var original = context.Tasks.Find(cloneOfId ?? 0);
            var model = (original == null)
                ? new EditModel
                {
                    Item = new Data.Tasks.ScheduledTask()
                    {
                        DefinitionId = definitionId ?? 0,
                        QueueName = "Main"
                    }
                }
                : new EditModel()
                {
                    Item = new Data.Tasks.ScheduledTask()
                    {
                        DefinitionId = original.DefinitionId,
                        Name = original.Name,
                        QueueName = original.QueueName,
                        MachineNameToRunOn = original.MachineNameToRunOn,
                        Arguments = original.Arguments,
                        UtcTimeToExecute = original.UtcTimeToExecute
                    },
                    HasChanges = true
                };

            return await EditView(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await context.Tasks
                .Include(k => k.Definition)
                .SingleOrDefaultAsync(k => k.Id == id);
            if (item == null) return new NotFoundResult();

            var model = new EditModel() { Item = item };

            if (model.Item.UtcTimeStarted.HasValue)
            {
                return ViewView(model);
            }
            else
            {
                return await EditView(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Submit([Bind(Prefix = "id")] int _, EditModel model)
        {
            ModelState.Clear();
            model.HasChanges = true;

            // Handle update of Task

            return await EditView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(int id, EditModel model, bool apply = false)
        {
            // Ignore errors on "Definition":
            ModelState.Remove("Item.Definition");

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(model.Item);
                    await context.SaveChangesAsync();
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
                    logger.LogError(ex, "Unexpected error saving task {ObjectId}", id);
                    ModelState.AddModelError("", "An unexpected error occured.");
                    ViewBag.Exception = ex;
                }
            }
            else
            {
                SetToastrMessage("error", "Failed to save the task.<br/>See validation messages for more information.");
            }

            Response.Headers["X-Sircl-History-Replace"] = Url.Action("Edit", new { id = model.Item!.Id });
            return await EditView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, EditModel model)
        {
            try
            {
                var item = await context.Tasks.FindAsync(id);
                if (item == null)
                {
                    this.SetToastrMessage("error", "Task not found.");
                }
                else if (item.UtcTimeStarted != null)
                {
                    this.SetToastrMessage("error", "Cannot delete a task that has already started.");
                }
                else
                {
                    context.Remove(item);
                    await context.SaveChangesAsync();
                    this.SetToastrMessage("success", "Task deleted");
                }
                return Back(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error deleting task {ObjectId}", id);
                ModelState.AddModelError("", "An unexpected error occured.");
                ViewBag.Exception = ex;
            }

            return await EditView(model);
        }

        [HttpPost]
        public async Task<IActionResult> ForceRestart(int id)
        {
            var item = await context.Tasks.FindAsync(id);
            if (item != null && item.UtcTimeDone == null)
            {
                item.UtcTimeStarted = null;
                item.Succeeded = null;
                item.OutputWriteLine($"=== {DateTime.UtcNow:yyyy/MM/yy HH:mm:ss} Restarted");
                await context.SaveChangesAsync();
            }

            return this.Back(false);
        }

        [HttpPost]
        public async Task<IActionResult> ForceAbort(int id)
        {
            var item = await context.Tasks.FindAsync(id);
            if (item != null && item.UtcTimeDone == null)
            {
                item.UtcTimeDone = DateTime.UtcNow;
                item.Succeeded = false;
                item.OutputWriteLine($"=== {DateTime.UtcNow:yyyy/MM/yy HH:mm:ss} Aborted");
                await context.SaveChangesAsync();
            }

            return this.Back(false);
        }

        private async Task<IActionResult> EditView(EditModel model)
        {
            // Load task definition:
            context.Tasks.Update(model.Item);
            context.Entry(model.Item).Reference(t => t.Definition).Load();

            // Retrieve all definitions:
            model.Definitions = await context.TaskDefinitions.OrderBy(d => d.Name).ToArrayAsync();
            if (model.Definitions.Length == 1) model.Item.DefinitionId = model.Definitions[0].Id;

            // Return the view:
            return View("Edit", model);
        }

        private IActionResult ViewView(EditModel model)
        {
            // Return the view:
            return View("View", model);
        }

        #endregion
    }
}
