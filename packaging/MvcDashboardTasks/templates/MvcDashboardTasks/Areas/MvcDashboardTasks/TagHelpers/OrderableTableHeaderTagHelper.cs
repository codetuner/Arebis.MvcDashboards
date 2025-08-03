﻿using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardTasks.TagHelpers
{
    /// <summary>
    /// Creates a sortable table header.
    /// Set the "asp-order" attribute with as value the current ordering expression to make the header sortable.
    /// Add a "field-name" attribute if the table header innertext is different from the field name.
    /// </summary>
    [HtmlTargetElement("th", Attributes = "asp-order")]
    public class OrderableTableHeaderTagHelper : TagHelper
    {
        /// <summary>
        /// Name of the form fields (by default "order").
        /// </summary>
        [HtmlAttributeName("name")]
        public string Name { get; set; } = "order";

        /// <summary>
        /// Name of the field.
        /// </summary>
        [HtmlAttributeName("field-name")]
        public string? FieldName { get; set; }

        /// <summary>
        /// Current order expression.
        /// </summary>
        [HtmlAttributeName("asp-order")]
        public string? CurrentOrder { get; set; }

        /// <summary>
        /// Processes the taghelper.
        /// </summary>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var originalContent = (await output.GetChildContentAsync()).GetContent();
            var fieldName = FieldName ?? originalContent;

            output.Attributes.Add("onclick-check", "> INPUT[name='" + Name + "']:not(:checked)");

            var builder = new StringBuilder();

            if (CurrentOrder == fieldName + " ASC")
            {
                builder.Append(originalContent);
                builder.Append(" <span class=\"xfloat-end\">&#9650;</span>");
                builder.Append("<input hidden type=\"radio\" name=\"" + Name + "\" value=\"" + fieldName + " ASC\" checked />");
                builder.Append("<input hidden type=\"radio\" name=\"" + Name + "\" value=\"" + fieldName + " DESC\" />");
            }
            else if (CurrentOrder == fieldName + " DESC")
            {
                builder.Append(originalContent);
                builder.Append(" <span class=\"xfloat-end\">&#9660;</span>");
                builder.Append("<input hidden type=\"radio\" name=\"" + Name + "\" value=\"" + fieldName + " DESC\" checked />");
                builder.Append("<input hidden type=\"radio\" name=\"" + Name + "\" value=\"" + fieldName + " ASC\" />");
            }
            else
            {
                builder.Append(originalContent);
                builder.Append(" <span class=\"xfloat-end\" style=\"color:#c0c0c0\">&#9650;</span>");
                builder.Append("<input hidden type=\"radio\" name=\"" + Name + "\" value=\"" + fieldName + " ASC\" />");
            }

            output.Content.SetHtmlContent(builder.ToString());
        }
    }
}
