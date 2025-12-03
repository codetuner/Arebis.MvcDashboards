using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MyMvcApp.Areas.MvcDashboardContent.Models.PathRedirection;
using MyMvcApp.Data.Content;
using System;
using System.Linq;

namespace MyMvcApp.Areas.MvcDashboardContent.Controllers
{
    [Authorize(Roles = "Administrator,ContentAdministrator")]
    public partial class PathRedirectionController : BaseController
    {
        #region Construction

        private readonly ContentDbContext context;
        private readonly IMemoryCache cache;

        public PathRedirectionController(ContentDbContext context, IMemoryCache cache)
        {
            this.context = context;
            this.cache = cache;
        }

        #endregion

        #region Index

        public IActionResult Index(IndexModel model)
        {
            var count = context.ContentPathRedirections
                .Where(i => i.FromPath.Contains(model.Query ?? "") || i.ToPath.Contains(model.Query ?? ""))
                .Count();
            model.MaxPage = (count + model.PageSize - 1) / model.PageSize;
            model.Items = context.ContentPathRedirections
                .Where(i => i.FromPath.Contains(model.Query ?? "") || i.ToPath.Contains(model.Query ?? ""))
                .OrderBy(model.Order ?? "FromPath ASC")
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToArray();

            return View("Index", model);
        }

        #endregion

        #region Edit

        [HttpGet]
        public IActionResult New()
        {
            var model = new EditModel();
            model.Item = Activator.CreateInstance<Data.Content.PathRedirection>();
            return EditView(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = new EditModel();
            model.Item = context.ContentPathRedirections.Find(id)
                ?? throw new BadHttpRequestException("Object not found.", 404);
            return EditView(model);
        }

        [HttpPost]
        public IActionResult Save(int id, EditModel model, bool apply = false)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(model.Item);
                    context.SaveChanges();
                    cache.Remove("Content:PathRedirections");
                    if (!apply) return Back(false);
                    else
                    {
                        ModelState.Clear();
                        model.HasChanges = false;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occured.");
                    ViewBag.Exception = ex;
                }
            }

            Response.Headers["X-Sircl-History-Replace"] = Url.Action("Edit", new { id = model.Item.Id });
            return EditView(model);
        }

        [HttpPost]
        public IActionResult Delete(int id, EditModel model)
        {
            try
            {
                var item = context.ContentSecuredPaths.Find(id)
                    ?? throw new BadHttpRequestException("Object not found.", 404);
                context.Remove(item);
                context.SaveChanges();
                cache.Remove("Content:PathRedirections");
                return Back(false);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occured.");
                ViewBag.Exception = ex;
            }

            return EditView(model);
        }

        private IActionResult EditView(EditModel model)
        {
            model.PathsList = context.ContentDocuments.Where(s => s.Path != null).Select(d => d.Path!).Distinct().OrderBy(p => p).ToList();
            return View("Edit", model);
        }

        #endregion
    }
}
