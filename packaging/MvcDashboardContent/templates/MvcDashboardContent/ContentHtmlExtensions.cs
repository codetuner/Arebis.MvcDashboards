using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyMvcApp.Models.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp
{
    public static class ContentHtmlExtensions
    {
        /// <summary>
        /// Convenience extension method that replaces
        /// @Html.DisplayFor(m => m.Property["Title"].Value, Model.Property["Title"].Type.DataType.Template)
        /// into
        /// @Html.Content("Title")
        /// </summary>
        public static IHtmlContent Content(this IHtmlHelper<ContentModel> htmlHelper, string propertyName, object? additionalViewData = null)
        {
            var property = htmlHelper.ViewData.Model.Document[propertyName];
            return htmlHelper.DisplayFor(m => m.Document[propertyName].Value, property.DataTypeTemplate, additionalViewData);
        }
    }
}
