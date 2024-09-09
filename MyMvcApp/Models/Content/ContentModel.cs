using MyMvcApp.Data.Content;

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
