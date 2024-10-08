﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardIdentity.TagHelpers
{
    [HtmlTargetElement("pagination-nav", Attributes = "asp-for, max")]
    public class PaginationNavTagHelper : TagHelper
    {
        // See: http://blog.techdominator.com/article/using-html-helper-inside-tag-helpers.html
        //private IHtmlHelper htmlHelper;

        //public PaginationTagHelper(IHtmlHelper htmlHelper)
        //{
        //    this.htmlHelper = htmlHelper;
        //}

        //[ViewContext]
        //[HtmlAttributeNotBound]
        //public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("asp-for")]
        public ModelExpression? For { get; set; }

        [HtmlAttributeName("min")]
        public int Min { get; set; } = 1;

        [HtmlAttributeName("max")]
        public int Max { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (this.For == null)
            {
                throw new NullReferenceException("PaginationNavTagHelper.For must have a value.");
            }

            //(htmlHelper as IViewContextAware).Contextualize(ViewContext);
            //var id = htmlHelper.GenerateIdFromName(AspFor.Name);
            var name = For.Name;
            var value = (int)For.Model;
    
            output.TagName = "nav";
            output.TagMode = TagMode.StartTagAndEndTag;

            var builder = new StringBuilder();
            builder.Append("<ul class=\"pagination\">");
            WritePage(builder, name, value, (value == Min ? Min - 1 : value - 1), "&laquo;", "ArrowLeft");
            if ((Max - Min) < 7)
            {
                for (int p = Min; p <= Max; p++)
                {
                    WritePage(builder, name, value, p, null, (p == Min) ? "1" : null);
                }
            }
            else
            {
                var pages = new int[] { value - 3, value - 2, value - 1, value, value + 1, value + 2, value + 3 };
                if (pages[0] < Min)
                {
                    var delta = Min - pages[0];
                    for (int i = 0; i < pages.Length; i++) pages[i] += delta;
                }
                else if (pages[^1] > Max)
                {
                    var delta = pages[^1] - Max;
                    for (int i = 0; i < pages.Length; i++) pages[i] -= delta;
                }
                if (pages[0] > Min)
                {
                    pages[0] = Min;
                    pages[1] = Min - 1;
                }
                if (pages[^1] < Max)
                {
                    pages[^2] = Min - 1;
                    pages[^1] = Max;
                }
                for (int i = 0; i < pages.Length; i++)
                {
                    WritePage(builder, name, value, pages[i], null, (pages[i] == Min) ? "1" : null);
                }
            }
            WritePage(builder, name, value, (value == Max ? Min - 1 : value + 1), "&raquo;", "ArrowRight");
            builder.Append("</ul>");

            output.Content.SetHtmlContent(builder.ToString());
        }

        private void WritePage(StringBuilder builder, string name, int value, int page, string? text = null, string? shortCut = null)
        {
            var active = (page == value);
            builder.Append($"<li class=\"page-item{(active ? " active" : "")}{((page < Min) ? " disabled" : "")}\">");
            builder.Append($"<label class=\"page-link\">");
            if (page >= Min) builder.Append($"<input type=\"radio\" name=\"{name}\" value=\"{page}\"{ ((shortCut == null) ? "" : $" onkeydown-click=\"{ shortCut }\"") } />");
            if (text != null) builder.Append(text);
            else if (page < Min) builder.Append("...");
            else builder.Append(page);
            builder.Append($"</label>");
            builder.Append($"</li>");
        }
    }
}
