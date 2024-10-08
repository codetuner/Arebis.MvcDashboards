﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Areas.MvcDashboardContent.Models.DocumentType;
using MyMvcApp.Data.Content;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyMvcApp.Areas.MvcDashboardContent.Controllers
{
    [Authorize(Roles = "Administrator,ContentAdministrator")]
    public class DocumentTypeController : BaseController
    {
        #region Construction

        private readonly ContentDbContext context;

        public DocumentTypeController(ContentDbContext context)
        {
            this.context = context;
        }

        #endregion

        #region Index

        [HttpGet]
        public IActionResult Index(IndexModel model)
        {
            var count = context.ContentDocumentTypes
                .Where(i => i.Name.Contains(model.Query ?? ""))
                .Count();
            model.MaxPage = (count + model.PageSize - 1) / model.PageSize;
            model.Items = context.ContentDocumentTypes
                .Include(dt => dt.Base)
                .Where(i => i.Name.Contains(model.Query ?? ""))
                .OrderBy(model.Order ?? "Name ASC")
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
            var model = new EditModel() { Item = new DocumentType() { Name = null!, IsInstantiable = true } };
            return EditView(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Build model:
            var model = new EditModel();
            model.Item = context.ContentDocumentTypes
                .Include(dt => dt.OwnPropertyTypes)
                //.ThenInclude(pt => pt.DataType)
                .SingleOrDefault(dt => dt.Id == id)
                ?? throw new BadHttpRequestException("Object not found.", 404);

            // Ensure property type display orders are in sequence:
            var nextDisplayOrder = 0;
            foreach (var property in model.Item.OwnPropertyTypes!.OrderBy(pt => pt.DisplayOrder).ThenBy(pt => pt.Name))
            {
                property.DisplayOrder = nextDisplayOrder++;
            }

            // Return view:
            return EditView(model);
        }

        [HttpPost]
        public IActionResult AddPropertyType(int id, EditModel model)
        {
            ModelStateBindPropertySettings(model);
            ModelState.Clear();
            model.HasChanges = true;

            model.Item.OwnPropertyTypes ??= new List<PropertyType>();
            var nextDisplayOrder = model.Item.OwnPropertyTypes.Select(pt => pt.DisplayOrder).DefaultIfEmpty(-1).Max() + 1;
            model.Item.OwnPropertyTypes.Add(new Data.Content.PropertyType() { DisplayOrder = nextDisplayOrder, Name = null!, DocumentType = null!, DataType = null! });

            return EditView(model);
        }

        [HttpPost]
        public IActionResult MovePropertyTypeUp(int id, EditModel model, int index)
        {
            ModelStateBindPropertySettings(model);
            ModelState.Clear();
            model.HasChanges = true;

            var displayOrder = model.Item.OwnPropertyTypes![index].DisplayOrder;
            foreach (var property in model.Item.OwnPropertyTypes)
            {
                if (property.DisplayOrder == (displayOrder - 1)) property.DisplayOrder++;
                else if (property.DisplayOrder == displayOrder) property.DisplayOrder--;
            }

            return EditView(model);
        }

        [HttpPost]
        public IActionResult MovePropertyTypeDown(int id, EditModel model, int index)
        {
            ModelStateBindPropertySettings(model);
            ModelState.Clear();
            model.HasChanges = true;

            var displayOrder = model.Item.OwnPropertyTypes![index].DisplayOrder;
            foreach (var property in model.Item.OwnPropertyTypes)
            {
                if (property.DisplayOrder == (displayOrder + 1)) property.DisplayOrder--;
                else if (property.DisplayOrder == displayOrder) property.DisplayOrder++;
            }

            return EditView(model);
        }

        [HttpPost]
        public IActionResult DeletePropertyType(int id, EditModel model, int index)
        {
            ModelStateBindPropertySettings(model);
            ModelState.Clear();
            model.HasChanges = true;

            var displayOrder = model.Item.OwnPropertyTypes![index].DisplayOrder;
            foreach (var property in model.Item.OwnPropertyTypes)
            {
                if (property.DisplayOrder > displayOrder) property.DisplayOrder--;
            }

            if (model.Item.OwnPropertyTypes[index].Id != 0)
            {
                model.PropertyTypesToDelete.Add(model.Item.OwnPropertyTypes[index].Id);
            }
            model.Item.OwnPropertyTypes.RemoveAt(index);

            return EditView(model);
        }

        [HttpPost]
        public IActionResult Save(int id, EditModel model)
        {
            ModelStateBindPropertySettings(model);
            if (ModelState.IsValid)
            {
                try
                {
                    // Update document type and property types:
                    context.Update(model.Item);

                    // Delete removed property types:
                    foreach (var ptid in model.PropertyTypesToDelete)
                    {
                        context.Attach(new Data.Content.PropertyType() { Id = ptid, Name = null!, DataType = null!, DocumentType = null! }).State = EntityState.Deleted;
                        foreach (var propertyToDelete in context.ContentProperties.Where(p => p.TypeId == ptid))
                        {
                            context.Remove(propertyToDelete);
                        }
                    }

                    // Save changes:
                    context.SaveChanges();

                    // Return back:
                    return Back(false);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occured.");
                    ViewBag.Exception = ex;
                }
            }

            return EditView(model);
        }

        [HttpPost]
        public IActionResult Delete(int id, EditModel model)
        {
            ModelStateBindPropertySettings(model);
            try
            {
                var item = context.ContentDocumentTypes.Find(id);
                if (item != null) context.Remove(item);
                context.SaveChanges();
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
            // Order owned properties according to DisplayOrder:
            model.Item.OwnPropertyTypes ??= new List<PropertyType>();
            model.Item.OwnPropertyTypes = model.Item.OwnPropertyTypes.OrderBy(pt => pt.DisplayOrder).ToList();

            // Retrieve reference data:
            model.DocumentTypes = context.ContentDocumentTypes.OrderBy(t => t.Name).ToArray();
            model.DataTypes = context.ContentDataTypes.OrderBy(t => t.Name).ToArray();
            model.DataTypesDict = model.DataTypes.ToDictionary(dt => dt.Id, dt => dt);

            // Return view:
            return View("Edit", model);
        }

        private void ModelStateBindPropertySettings(EditModel model)
        {
            foreach(var pair in Request.Form.Where(p => p.Key.StartsWith("OwnPropertyTypesSettingsAsString[")))
            {
                var index = Int32.Parse(pair.Key[33..^1]);
                model.OwnPropertyTypesSettingsAsString[index] = pair.Value.ToString();
            }
        }

        #endregion
    }
}
