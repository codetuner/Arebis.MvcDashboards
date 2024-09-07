using MyMvcApp.Data.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Models.Content
{
    /// <summary>
    /// Model of a content document to render.
    /// </summary>
    public class ContentModel
    {
        /// <summary>
        /// The document to render content for.
        /// </summary>
        public required PublishedDocument Document { get; set; }
    }
}
