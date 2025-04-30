using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyMvcApp.Areas.MvcDashboardLocalize.Models.Domain;
using MyMvcApp.Data.Localize;
using System;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Xml;

namespace MyMvcApp.Areas.MvcDashboardLocalize.Controllers
{
    public class DomainController : BaseController
    {
        #region Construction

        private readonly LocalizeDbContext context;
        private readonly ILogger logger;

        public DomainController(LocalizeDbContext context, ILogger<DomainController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        #endregion

        #region Index

        [HttpGet]
        public IActionResult Index(IndexModel model)
        {
            model.MaxPage = 1;
            model.Items = context.LocalizeDomains
                .OrderBy(i => i.Name).ThenBy(i => i.Id)
                .ToArray();

            return View("Index", model);
        }

        #endregion

        #region Edit

        public IActionResult New([FromServices] IOptions<RequestLocalizationOptions> requestLocalizationOptions)
        {
            var model = new EditModel
            {
                Item = new Data.Localize.Domain()
            };
            if (requestLocalizationOptions.Value.SupportedUICultures != null)
                model.Cultures = String.Join(',', requestLocalizationOptions.Value.SupportedUICultures.Select(c => c.TwoLetterISOLanguageName).Distinct());

            return EditView(model);
        }

        public IActionResult Edit(int id)
        {
            var item = context.LocalizeDomains.Find(id);
            if (item == null) return new NotFoundResult();

            var model = new EditModel() { Item = item };
            model.Cultures = String.Join(", ", model.Item.Cultures ?? Array.Empty<string>());

            return EditView(model);
        }

        [HttpPost]
        public IActionResult Save(int id, EditModel model)
        {
            // Validate cultures:
            if (model.Cultures != null)
            {
                var cultures = model.Cultures.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
                if (cultures.Distinct().Count() != cultures.Length)
                {
                    ModelState.AddModelError("Cultures", "Value should not contain duplicates!");
                }
                else if (cultures.Length == 0)
                {
                    ModelState.AddModelError("Cultures", "Value is required!");
                }
                else
                { 
                    model.Item.Cultures = cultures;
                }
            }
            else
            {
                ModelState.AddModelError("Cultures", "Value is required!");
            }

            // Validate modelstate and save:
            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(model.Item);
                    context.SaveChanges();
                    return this.Close(true);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error saving domain {domainid}", id);
                    ModelState.AddModelError("", "An unexpected error occured.");
                    ViewBag.Exception = ex;
                }
            }

            return EditView(model);
        }

        [HttpPost]
        public IActionResult Delete(int id, EditModel model)
        {
            try
            {
                var item = context.LocalizeDomains.Find(id);
                if (item != null)
                {
                    context.Remove(item);
                    context.SaveChanges();
                }
                return this.Close(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error deleting domain {domainid}", id);
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

        #region Import

        [HttpPost]
        public IActionResult Import(IFormFile file)
        {
            if (!User.IsAdministrator())
            {
                return Forbid();
            }

            using (var stream = file.OpenReadStream())
            { 
                var domain = JsonSerializer.Deserialize<Data.Localize.Domain>(stream);
                if (domain != null)
                {
                    context.LocalizeDomains.Add(domain);
                    context.SaveChanges();
                    SetToastrMessage("success", $"Domain {domain.Name} imported with Id {domain.Id}.");
                }
            }

            return Index(new IndexModel());
        }

        #endregion

        #region Export

        public IActionResult Export(int id)
        {
            if (!User.IsAdministrator())
            {
                return Forbid();
            }

            var domain = context.LocalizeDomains
                .Include(d => d.Keys!).ThenInclude(k => k.Values)
                .Include(d => d.Queries)
                .Single(d => d.Id == id);

            var json = JsonSerializer.Serialize(domain);
            return this.Content(json, MediaTypeNames.Application.Json);
        }

        #endregion

        #region ExportToTMX

        [HttpGet]
        public IActionResult ExportToTmx(int id)
        {
            var model = new ExportToTmxModel
            {
                DomainId = id,
                IncludeNonReviewed = false,
                IncludeNotes = false
            };

            return View(model);
        }

        [HttpPost]
        public void ExportToTmx([Bind(Prefix = "id")] int domainId, ExportToTmxModel model, CancellationToken ct)
        {
            // Retrieve domain:
            var domain = context.LocalizeDomains.AsNoTracking()
                .Include(d => d.Keys!).ThenInclude(k => k.Values)
                .Single(d => d.Id == domainId);

            // Build Xml document:
            var doc = new System.Xml.XmlDocument();
            var root = (XmlElement)doc.AppendChild(doc.CreateElement("tmx").WithAttribute("version", "1.4"))!;
            var header = root.AppendChild(doc.CreateElement("header")
                .WithAttribute("creationtool", "MvcDashboardLocalize")
                .WithAttribute("creationtoolversion", "1.0"))!;
            header.AppendChild(doc.CreateElement("prop").WithAttribute("type", "x-Domain").WithText(domain.Name));
            var body = root.AppendChild(doc.CreateElement("body"))!;
            foreach (var key in domain.Keys!)
            {
                if (ct.IsCancellationRequested) return;
                var keyValues = key.Values!
                    .Where(v => (model.IncludeNonReviewed == true || v.Reviewed == true) && (!String.IsNullOrWhiteSpace(v.Value)))
                    .ToList();
                if (keyValues.Count < 1) continue;
                var tu = body.AppendChild(doc.CreateElement("tu")
                    .WithAttribute("tuid", key.Name ?? "")
                    .WithAttribute("datatype", key.MimeType == "text/html" ? "html" : "plaintext"))!;
                if (!String.IsNullOrWhiteSpace(key.Notes))
                    tu.AppendChild(doc.CreateElement("note").WithText(key.Notes));
                if (!String.IsNullOrWhiteSpace(key.ForPath))
                    tu.AppendChild(doc.CreateElement("prop").WithAttribute("type", "x-ForPath").WithText(key.ForPath));
                if (key.ArgumentNames != null && key.ArgumentNames.Length > 0)
                    tu.AppendChild(doc.CreateElement("prop").WithAttribute("type", "x-ArgumentNames").WithText(String.Join(' ', key.ArgumentNames)));
                foreach (var value in keyValues)
                {
                    var tuv = tu.AppendChild(doc.CreateElement("tuv").WithAttribute("lang", value.Culture))!;
                    if (!value.Reviewed)
                        tuv.AppendChild(doc.CreateElement("prop").WithAttribute("type", "x-ToReview").WithText("true"));
                    tuv.AppendChild(doc.CreateElement("seg").WithText(value.Value!));
                }
            }

            // Return result:
            Response.StatusCode = 200;
            Response.Headers.ContentType = MediaTypeNames.Text.Xml;
            Response.Headers.ContentDisposition = $"attachment; filename={domain.Name}.tmx";
            using (var writer = XmlWriter.Create(Response.BodyWriter.AsStream(false)))
            {
                doc.WriteContentTo(writer);
            }
        }

        #endregion
    }
}
