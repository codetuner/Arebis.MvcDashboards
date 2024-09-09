using Arebis.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Areas.MvcDashboardLocalize.Models.Key;
using MyMvcApp.Data.Localize;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;

namespace MyMvcApp.Areas.MvcDashboardLocalize.Controllers
{
    public class KeyController : BaseController
    {
        #region Construction

        private readonly LocalizeDbContext context;
        private readonly ILogger logger;
        private readonly ITranslationService? translationService;

        public KeyController(LocalizeDbContext context, ILogger<KeyController> logger, ITranslationService? translationService = null)
        {
            this.context = context;
            this.logger = logger;
            this.translationService = translationService;
        }

        #endregion

        #region Index

        [HttpGet]
        public async Task<IActionResult> Index(IndexModel model)
        {
            var noQuery = String.IsNullOrWhiteSpace(model.Query);
            var count = await context.LocalizeKeys
                .Where(i => i.DomainId == model.DomainId || model.DomainId == null)
                .Where(i => noQuery || i.Name!.Contains(model.Query ?? "") || i.ForPath!.Contains(model.Query ?? "") || i.Values!.Any(v => v.Value!.Contains(model.Query ?? "")))
                .CountAsync();
            model.MaxPage = (count + model.PageSize - 1) / model.PageSize;
            model.Items = await context.LocalizeKeys
                .Include(i => i.Domain)
                .Where(i => i.DomainId == model.DomainId || model.DomainId == null)
                .Where(i => noQuery || i.Name!.Contains(model.Query ?? "") || i.ForPath!.Contains(model.Query ?? "") || i.Values!.Any(v => v.Value!.Contains(model.Query ?? "")))
                .OrderBy(model.Order ?? "Name ASC, ForPath ASC, DomainId ASC")
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToArrayAsync();
            model.Domains = await context.LocalizeDomains
                .OrderBy(d => d.Name).ThenBy(d => d.Id)
                .Select(d => new SelectListItem() { Value = d.Id.ToString(), Text = d.Name, Selected = (model.DomainId == d.Id) })
                .ToListAsync();

            return View("Index", model);
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> New(int? domainId)
        {
            var model = new EditModel
            {
                Item = new Data.Localize.Key() { DomainId = domainId ?? 0, MimeType = MediaTypeNames.Text.Plain }
            };

            return await EditView(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await context.LocalizeKeys
                .Include(k => k.Domain)
                .Include(k => k.Values)
                .SingleOrDefaultAsync(k => k.Id == id);
            if (item == null) return new NotFoundResult();

            var model = new EditModel() { Item = item };
            model.ArgumentNames = String.Join(", ", model.Item.ArgumentNames ?? Array.Empty<string>());
            model.Values = model.Item.Values!.ToList();

            return await EditView(model);
        }

        [HttpPost]
        public IActionResult Preview(int id, EditModel model, string previewCulture)
        {
            return Content(model.Values.Single(v => v.Culture == previewCulture).Value ?? "(Empty)", MediaTypeNames.Text.Html);
        }

        [HttpPost]
        public async Task<IActionResult> AutoTranslate(int id, EditModel model, CancellationToken cancellationToken)
        {
            if (this.translationService == null)
            {
                throw new NotSupportedException("AutoTranslate requires an ITranslationService to be installed.");
            }

            // Check MimeType (Plain text or Html) is given:
            if (String.IsNullOrEmpty(model.Item?.MimeType))
            {
                SetToastrMessage("error", "Select content type then retry.");
                return await EditView(model);
            }

            ModelState.Clear();

            var succeededTranslations = new List<string>();
            var failedTranslations = new List<string>();
            var source = model.Values.Single(v => v.Culture == model.SourceCulture);
            var arguments = (model.ArgumentNames ?? "").Split(',').Select(a => a.Trim()).ToArray();
            if (!String.IsNullOrWhiteSpace(source.Value))
            {
                // Mark source as reviewed (as it is sufficiently trusted to base translations on):
                source.Reviewed = true;

                // Translate each culture that is not empty and not reviewed:
                var valuesToTranslate = model.Values
                    .Where(v => v.Culture != model.SourceCulture && v.Reviewed == false && String.IsNullOrWhiteSpace(v.Value))
                    .ToList();
                if (valuesToTranslate.Any())
                {
                    var result = (await translationService.TranslateAsync(source.Culture, valuesToTranslate.Select(v => v.Culture), model.Item.MimeType, ToTranslatable(source.Value, arguments), cancellationToken)).ToList();
                    for (int i = 0; i < valuesToTranslate.Count; i++)
                    {
                        if (result[i] != null)
                        {
                            valuesToTranslate[i].Value = FromTranslatable(result[i], arguments);
                            model.HasChanges = true;
                            succeededTranslations.Add(valuesToTranslate[i].Culture);
                        }
                        else
                        {
                            failedTranslations.Add(valuesToTranslate[i].Culture);
                        }
                    }
                }
            }

            if (failedTranslations.Any())
            {
                SetToastrMessage($"warning", $"Key translated with errors for {(String.Join(", ", failedTranslations))}.");
            }
            else if (succeededTranslations.Any())
            {
                SetToastrMessage("success", $"Key successfully translated in {(String.Join(", ", succeededTranslations))}.");
            }
            else
            {
                SetToastrMessage("info", "Nothing to translate.<br/>Only empty, non-reviewed cultures are translated.");
            }

            return await EditView(model);
        }

        /// <summary>
        /// In the given input string, replaces "{{FirstName}}" by "⁑2⁑" if "FirstName" is listed as the 3th argument.
        /// This to avoid translation of the "FirstName" argument name.
        /// </summary>
        [return: NotNullIfNotNull(nameof(input))]
        private string? ToTranslatable(string? input, string[] arguments)
        {
            if (input == null) return input;

            var output = input;
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i].Length == 0) continue;
                output = output.Replace("{{" + arguments[i] + "}}", "⁑" + i + "⁑");
            }

            return output;
        }

        /// <summary>
        /// Reverses the transformation done by <see cref="ToTranslatable(string?, string[])"/> method.
        /// </summary>
        [return:NotNullIfNotNull(nameof(input))]
        private string? FromTranslatable(string? input, string[] arguments)
        {
            if (input == null) return input;

            var output = input;
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i].Length == 0) continue;
                output = output.Replace("⁑" + i + "⁑", "{{" + arguments[i] + "}}");
            }

            return output;
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllReviewed(int id, EditModel model)
        {
            ModelState.Clear();

            foreach (var value in model.Values)
            {
                if (!value.Reviewed && !String.IsNullOrEmpty(value.Value))
                {
                    value.Reviewed = true;
                    model.HasChanges = true;
                }
            }

            return await EditView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int id, EditModel model)
        {
            ModelState.Clear();
            model.HasChanges = true;

            // Handle update of Domain, resulting in new list of cultures!

            return await EditView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(int id, EditModel model, bool apply = false, bool andcopy = false)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var domain = await context.LocalizeDomains.FindAsync(model.Item!.DomainId);

                    model.Item.ArgumentNames = string.IsNullOrWhiteSpace(model.ArgumentNames)
                        ? null
                        : model.ArgumentNames.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
                    model.Item.Values = model.Values.Where(v => v.Reviewed || v.Value != null).ToList();
                    foreach (var value in model.Values.Where(v => !v.Reviewed && v.Value == null && v.Id != default)) context.Remove(value);
                    model.Item.ValuesToReview = (domain?.Cultures ?? Array.Empty<string>()).Except(model.Item.Values.Where(v => v.Reviewed).Select(v => v.Culture)).ToArray();
                    context.Update(model.Item);
                    await context.SaveChangesAsync();

                    if (andcopy)
                    {
                        ModelState.Clear();
                        model.HasChanges = true;
                        model.Item!.Id = 0;
                        foreach (var v in model.Values) v.Id = 0;
                        Response.Headers["X-Sircl-History-Replace"] = Url.Action("New");
                    }
                    else if (apply)
                    {
                        ModelState.Clear();
                        model.HasChanges = false;
                        model.SaveAsCopy = false;
                        Response.Headers["X-Sircl-History-Replace"] = Url.Action("Edit", new { id = model.Item!.Id });
                    }
                    else
                    {
                        return Back(false);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error saving key {ObjectId}", id);
                    ModelState.AddModelError("", "An unexpected error occured.");
                    ViewBag.Exception = ex;
                }
            }
            else
            {
                SetToastrMessage("error", "Failed to save the key.<br/>See validation messages for more information.");
            }

            return await EditView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, EditModel model)
        {
            try
            {
                var item = await context.LocalizeKeys.FindAsync(id);
                if (item != null)
                {
                    context.Remove(item);
                    await context.SaveChangesAsync();
                }
                return Back(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error deleting key {ObjectId}", id);
                ModelState.AddModelError("", "An unexpected error occured.");
                ViewBag.Exception = ex;
            }

            return await EditView(model);
        }

        private async Task<IActionResult> EditView(EditModel model)
        {
            model.HasTranslationService = (this.translationService != null);

            model.Domains = await context.LocalizeDomains.OrderBy(d => d.Name).ToArrayAsync();
            if (model.Domains.Length == 1) model.Item.DomainId = model.Domains[0].Id;

            var domain = model.Domains.SingleOrDefault(d => d.Id == model.Item.DomainId);
            if (domain != null)
            {
                var values = model.Values.ToList();
                model.Values.Clear();
                foreach (var c in domain.Cultures ?? Array.Empty<string>())
                {
                    model.Values.Add(values.SingleOrDefault(v => v.Culture == c) ?? new KeyValue() { Culture = c });
                }
                foreach (var value in values)
                {
                    if (!model.Values.Contains(value) && (value.Reviewed || value.Value != null)) model.Values.Add(value);
                }
            }

            // Parse argument names for editor assistants:
            model.Item.ArgumentNames = string.IsNullOrWhiteSpace(model.ArgumentNames)
                ? null
                : model.ArgumentNames.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();

            // Return the view:
            return View("Edit", model);
        }

        #endregion
    }
}
