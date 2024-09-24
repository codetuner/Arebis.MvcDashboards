using Arebis.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyMvcApp.Areas.MvcDashboardContent.Models.Document;
using MyMvcApp.Data.Content;
using MyMvcApp.Models.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardContent.Controllers
{
    [Authorize(Roles = "Administrator,ContentAdministrator,ContentEditor,ContentAuthor")]
    public class DocumentController : BaseController
    {
        #region Construction

        private readonly ContentDbContext context;
        private readonly IOptions<RequestLocalizationOptions> localizationOptions;
        private readonly ITranslationService? translationService;

        public DocumentController(ContentDbContext context, IOptions<RequestLocalizationOptions> localizationOptions, ITranslationService? translationService = null)
        {
            this.context = context;
            this.localizationOptions = localizationOptions;
            this.translationService = translationService;
        }

        #endregion

        #region Index

        [HttpGet]
        public IActionResult Index(IndexModel model)
        {
            var stateExpression = model.State switch
            {
                "new" => (Expression<Func<Document, bool>>)(d => d.DeletedOnUtc == null && d.LastPublishedOnUtc == null),
                "outdated" => (Expression<Func<Document, bool>>)(d => d.DeletedOnUtc == null && d.LastPublishedOnUtc != null && d.IsLatestPublished == false),
                "uptodate" => (Expression<Func<Document, bool>>)(d => d.DeletedOnUtc == null && d.LastPublishedOnUtc != null && d.IsLatestPublished == true),
                "deleted" => (Expression<Func<Document, bool>>)(d => d.DeletedOnUtc != null),
                _ => (Expression<Func<Document, bool>>)(d => d.DeletedOnUtc == null),
            };

            var count = context.ContentDocuments
                .Where(d => d.Name.Contains(model.Query ?? "") || d.Path!.Contains(model.Query ?? ""))
                .Where(d => d.TypeId == model.DocumentTypeId || model.DocumentTypeId == null)
                .Where(stateExpression)
                .Count();
            model.MaxPage = (count + model.PageSize - 1) / model.PageSize;
            model.Items = context.ContentDocuments
                .Include(d => d.Type)
                .Where(d => d.Name.Contains(model.Query ?? "") || d.Path!.Contains(model.Query ?? ""))
                .Where(d => d.TypeId == model.DocumentTypeId || model.DocumentTypeId == null)
                .Where(stateExpression)
                .OrderBy(model.Order ?? "Name ASC")
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToArray();
            model.DocumentTypes = context.ContentDocumentTypes
                .Where(dt => dt.IsInstantiable)
                .OrderBy(dt => dt.Name)
                .Select(dt => new SelectListItem() { Value = dt.Id.ToString(), Text = dt.Name, Selected = (model.DocumentTypeId == dt.Id) })
                .ToList();
            model.States = new List<SelectListItem>();
            model.States.Add(new SelectListItem("New (never published)", "new"));
            model.States.Add(new SelectListItem("Published but has newer version", "outdated"));
            model.States.Add(new SelectListItem("Published (no newer version)", "uptodate"));
            model.States.Add(new SelectListItem("Deleted", "deleted"));

            return View("Index", model);
        }

        [HttpGet]
        public IActionResult NewModal()
        {
            var model = new NewModel();
            model.DocumentTypes = context.ContentDocumentTypes.Where(dt => dt.IsInstantiable).OrderBy(dt => dt.Name).ToList();

            return View("NewModal", model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,ContentAdministrator,ContentEditor")]
        public IActionResult IndexPublish(IndexModel model, int[] selection, CancellationToken ct)
        {
            context.ContentDocuments
                .Where(d => selection.Contains(d.Id) && d.IsLatestPublished == false)
                .ToList()
                .ForEach(d => d.PublishAsync(context, this.HttpContext.User.Identity, false, ct).Wait());

            context.SaveChanges();

            return Index(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,ContentAdministrator,ContentEditor")]
        public IActionResult IndexUnpublish(IndexModel model, int[] selection, CancellationToken ct)
        {
            context.ContentDocuments
                .Where(d => selection.Contains(d.Id))
                .ToList()
                .ForEach(d => d.UnpublishAsync(context, this.HttpContext.User.Identity, ct).Wait());

            context.SaveChanges();

            return Index(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,ContentAdministrator,ContentEditor")]
        public IActionResult IndexDelete(IndexModel model, int[] selection, CancellationToken ct)
        {
            var user = this.HttpContext.User;
            var userName = user.Identity?.Name;

            context.ContentDocuments
                .Where(d => selection.Contains(d.Id))
                .ToList()
                .ForEach(d => d.DeleteAsync(context, user.Identity, ct).Wait());

            context.SaveChanges();

            return Index(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,ContentAdministrator,ContentEditor")]
        public IActionResult IndexRestore(IndexModel model, int[] selection, CancellationToken ct)
        {
            var user = this.HttpContext.User;
            var userName = user.Identity?.Name;

            context.ContentDocuments
                .Where(d => selection.Contains(d.Id))
                .ToList()
                .ForEach(d => d.Restore(context, user.Identity));

            context.SaveChanges();

            return Index(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,ContentAdministrator,ContentEditor")]
        public async Task<IActionResult> IndexDestroy(IndexModel model, int[] selection, CancellationToken ct)
        {
            await context.ContentDocuments
                .Where(d => selection.Contains(d.Id) && d.DeletedOnUtc != null)
                .ExecuteDeleteAsync(ct);

            return Index(model);
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> New(int? typeId = null, string? culture = null, string? path = null, CancellationToken ct = default)
        {
            var model = new EditModel();
            model.Item = Activator.CreateInstance<Data.Content.Document>();
            if (this.localizationOptions.Value.SupportedUICultures != null && this.localizationOptions.Value.SupportedUICultures.Count > 1)
                model.Item.Culture = this.localizationOptions.Value.DefaultRequestCulture.UICulture.Name;
            model.Item.TypeId = typeId ?? 0;
            model.Item.Path = path;

            return await EditView(model, ct);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var model = new EditModel();
            model.Item = context.ContentDocuments
                .Include(d => d.Properties)
                .SingleOrDefault(d => d.Id == id)
                ?? throw new BadHttpRequestException("Object not found.", 404);
            model.IsDeleted = model.Item.DeletedOnUtc.HasValue;

            return await EditView(model, ct);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int id, EditModel model, CancellationToken ct)
        {
            ModelState.Clear();
            model.HasChanges = true;

            return await EditView(model, ct);
        }

        [HttpPost]
        public async Task<IActionResult> Preview(int id, EditModel model, CancellationToken ct = default)
        {
            context.Attach(model.Item);
            var documentType = await context.ContentDocumentTypes.FindAsync(model.Item.TypeId, ct);
            var publishedDocument = await model.Item.PublishAsync(context, null, true, ct);
            ViewBag.IsPreview = true;
            return View($"~/Views/Content/{documentType?.ViewName ?? "NotFound"}.cshtml", new ContentModel() { Document = publishedDocument });
        }

        [HttpPost]
        public async Task<IActionResult> Save(int id, EditModel model, bool apply = false, bool andcopy = false, string? andtranslate = null, CancellationToken ct = default)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update document:
                    model.Item.Save(context, this.HttpContext.User?.Identity);
                    await context.SaveChangesAsync(ct);

                    // Also publish if requested so:
                    if (model.Publish || model.Item.AutoPublish)
                    {
                        await context.ContentDocuments.Find(model.Item.Id)!.PublishAsync(context, this.HttpContext.User?.Identity, false, ct);
                        await context.SaveChangesAsync(ct);
                        model.Publish = false;
                    }

                    // Return answer:
                    if (apply)
                    {
                        ModelState.Clear();
                        model.HasChanges = false;
                        Response.Headers["X-Sircl-History-Replace"] = Url.Action("Edit", new { id = model.Item.Id });
                    }
                    else if (andcopy)
                    {
                        ModelState.Clear();
                        model.HasChanges = true;
                        model.Item.Id = 0;
                        model.Item.Properties?.ForEach(p => p.Id = 0);
                        model.Item.AutoPublish = false;
                        Response.Headers["X-Sircl-History-Replace"] = Url.Action("New");
                    }
                    else if (andtranslate != null && this.translationService != null && model.Item.Culture != null)
                    {
                        ModelState.Clear();
                        model.HasChanges = true;
                        model.Item.Id = 0;
                        model.Item.Properties?.ForEach(p => p.Id = 0);
                        Response.Headers["X-Sircl-History-Replace"] = Url.Action("New");

                        // Translate properties:
                        var fromLanguage = model.Item.Culture;
                        model.Item.Culture = andtranslate;
                        foreach (var property in model.Item.Properties ?? [])
                        {
                            if (String.IsNullOrWhiteSpace(property.Value)) continue;
                            var type = await context.ContentPropertyTypes.FindAsync(property.TypeId, ct);
                            if (type == null) continue;
                            await context.Entry(type).Reference(e => e.DataType).LoadAsync(ct);
                            if (type.CombinedSettings.TryGetValue("CultureSensitive", out object? css))
                            {
                                if (Boolean.Parse((string)css ?? "false") == true)
                                {
                                    var translation = await this.translationService.TranslateAsync(fromLanguage, andtranslate, type.CombinedSettings.ContainsKey("MimeType") ? (string)type.CombinedSettings["MimeType"] : "text/text", new string[] { property.Value }, ct);
                                    property.Value = translation.FirstOrDefault();
                                }
                            }
                        }
                    }
                    else
                    {
                        return Back(false);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occured.");
                    ViewBag.Exception = ex;
                }
            }

            return await EditView(model, ct);
        }

        [HttpPost]
        public async Task<IActionResult> SavePropertyValue(int id, int propertyId, string value, CancellationToken ct = default)
        {
            var document = (await context.ContentDocuments
                .Include(d => d.Properties)
                .SingleOrDefaultAsync(d =>d.Id == id, ct))
                ?? throw new BadHttpRequestException("Object not found.", 404);
            document.Properties!.Single(p => p.Id == propertyId).Value = value;
            document.ModifiedOnUtc = DateTime.UtcNow;
            document.ModifiedBy = this.HttpContext.User?.Identity?.Name;
            document.IsLatestPublished = false;
            await context.SaveChangesAsync(ct);

            // Also publish if document is set to autopublish:
            if (document.AutoPublish)
            {
                await context.ContentDocuments.Find(document.Id)!.PublishAsync(context, this.HttpContext.User?.Identity, false, ct);
                await context.SaveChangesAsync(ct);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, EditModel model, CancellationToken ct = default)
        {
            try
            {
                var item = context.ContentDocuments.Find(id)
                    ?? throw new BadHttpRequestException("Object not found.", 404);
                await item.DeleteAsync(context, this.HttpContext.User.Identity, ct);
                await context.SaveChangesAsync();

                return Back(false);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occured.");
                ViewBag.Exception = ex;
            }

            return await EditView(model, ct);
        }

        private async Task<IActionResult> EditView(EditModel model, CancellationToken ct)
        {
            model.Item.Properties ??= new();
            model.AllDocumentTypes = context.ContentDocumentTypes
                .Include(dt => dt.OwnPropertyTypes!).ThenInclude(pt => pt.DataType)
                .OrderBy(t => t.Name).ToArray();
            model.AllDocumentTypesDict = model.AllDocumentTypes.ToDictionary(dt => dt.Id, dt => dt);
            if (model.Item!.TypeId != 0)
                model.DocumentType = model.AllDocumentTypesDict[model.Item!.TypeId];
            if (this.localizationOptions.Value.SupportedUICultures != null && this.localizationOptions.Value.SupportedUICultures.Count > 1)
            {
                var docCultures = await context.ContentDocuments.Where(d => d.Name == model.Item!.Name && d.Id != model.Item!.Id && d.DeletedOnUtc == null)
                    .Select(d => d.Culture).ToListAsync(ct);
                model.SupportedUICultures = this.localizationOptions.Value.SupportedUICultures.Where(c => !docCultures.Contains(c.Name)).ToList();
            }
            model.PathsList = context.ContentDocuments.Where(d => d.Path != null).Select(d => d.Path!).Distinct().OrderBy(p => p).ToList();
            model.HasTranslationService = this.translationService != null;
            return View("Edit", model);
        }

        #endregion
    }
}
