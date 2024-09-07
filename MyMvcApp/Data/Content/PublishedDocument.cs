using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Identity;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MyMvcApp.Data.Content
{
    /// <summary>
    /// A published content document.
    /// </summary>
    [Table(nameof(PublishedDocument), Schema = "content")]
    public class PublishedDocument
    {
        /// <summary>
        /// Identifier of the published document.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        /// <summary>
        /// Identifier of the origin document.
        /// </summary>
        public virtual int DocumentId { get; set; }

        /// <summary>
        /// The origin document.
        /// </summary>
        [ForeignKey(nameof(DocumentId))]
        public virtual Document? Document {  get; set; }

        /// <summary>
        /// Sequential version number.
        /// </summary>
        public virtual int Version { get; set; }

        /// <summary>
        /// Name of the document.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual required string Name { get; set; }

        /// <summary>
        /// Culture of the document.
        /// </summary>
        [MaxLength(200)]
        public virtual string? Culture { get; set; }

        /// <summary>
        /// SortKey used to sort child douments of the same parent (according to path).
        /// </summary>
        [MaxLength(200)]
        public virtual string? SortKey { get; set; }

        /// <summary>
        /// Path of the document.
        /// </summary>
        [MaxLength(2000)]
        public virtual string? Path { get; set; }

        /// <summary>
        /// [NotMapped] Segments the path is made of, provided the path starts with a "/".
        /// I.e. if the path is "/Home/About", would return ["Home", "About"].
        /// </summary>
        [NotMapped]
        public string[]? PathSegments
        {
            get
            {
                if (this.Path != null && this.Path.StartsWith("/"))
                {
                    if (this.Path.Length == 1)
                    {
                        return Array.Empty<string>();
                    }
                    else
                    {
                        return this.Path[0..^1].Split('/');
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Number of segments that make up the path, provided the path starts with a "/".
        /// Used for querying direct children quickly.
        /// </summary>
        public virtual int? PathSegmentsCount { get; set; }

        /// <summary>
        /// Returns the path of the parent.
        /// Returns null if the document has no path or is root.
        /// </summary>
        [NotMapped]
        public string? ParentPath
        {
            get
            {
                if (this.PathSegments == null) return null;
                var ps = this.PathSegments;
                if (ps.Length == 0) return null;
                else if (ps.Length == 1) return "/";
                else return '/' + String.Join('/', ps.Take(ps.Length - 1));
            }
        }

        /// <summary>
        /// Document type name.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual required string DocumentTypeName { get; set; }

        /// <summary>
        /// (Razor) view to use to render this document.
        /// </summary>
        [MaxLength(200)]
        public virtual string? ViewName { get; set; }

        /// <summary>
        /// Properties of this document.
        /// </summary>
        public virtual List<PublishedDocumentProperty> Properties { get; set; } = [];

        /// <summary>
        /// [NotMapped] Returns the property with the given name.
        /// </summary>
        [NotMapped]
        public PublishedDocumentProperty this[string name]
        {
            get
            {
                return this.Properties.FirstOrDefault(p => p.Name == name)
                    ?? throw new KeyNotFoundException($"No property with name \"{name}\" found.");
            }
        }

        /// <summary>
        /// Returns all PublishedDocuments (including this one) under the same parent as this one.
        /// If this document has no path, an empty list is returned.
        /// </summary>
        public IOrderedQueryable<PublishedDocument> GetSiblings(ContentDbContext context)
        {
            if (this.PathSegmentsCount == null) return new List<PublishedDocument>().AsQueryable().OrderBy(d => d.Id);

            return context.ContentPublishedDocuments.AsNoTracking()
                .Where(d => d.PathSegmentsCount == this.PathSegmentsCount && d.Path!.StartsWith(this.ParentPath! + '/'))
                .OrderBy(d => d.SortKey).ThenBy(d => d.Name).ThenBy(d => d.Id);
        }

        /// <summary>
        /// Returns the ancestors (parents, grand-parents, ...) of this document.
        /// If this document has no path, an empty list is returned.
        /// </summary>
        public IQueryable<PublishedDocument> GetAncestors(ContentDbContext context)
        {
            if (this.PathSegmentsCount == null) return new List<PublishedDocument>().AsQueryable().OrderBy(d => d.Id);

            var pathSegments = this.PathSegments!;
            var parentPaths = new List<string>();
            for(int i=0; i < this.PathSegmentsCount; i++)
            {
                parentPaths.Add('/' + String.Join('/', pathSegments.Take(i)));
            }

            return context.ContentPublishedDocuments.AsNoTracking()
                .Where(d => parentPaths.Contains(d.Path!));
        }
        
        /// <summary>
        /// Returns the direct parents of this document.
        /// If this document has no path or is root, an empty list is returned.
        /// </summary>
        public IOrderedQueryable<PublishedDocument> GetParents(ContentDbContext context)
        {
            if (this.PathSegmentsCount == null) return new List<PublishedDocument>().AsQueryable().OrderBy(d => d.Id);
            if (this.PathSegmentsCount == 0) return new List<PublishedDocument>().AsQueryable().OrderBy(d => d.Id);

            var parentPath = '/' + String.Join('/', this.PathSegments!.Take(this.PathSegmentsCount.Value - 1));
            return context.ContentPublishedDocuments.AsNoTracking()
                .Where(d => d.Path == parentPath)
                .OrderBy(d => d.SortKey).ThenBy(d => d.Name).ThenBy(d => d.Id);
        }

        /// <summary>
        /// Returns the direct children of this document.
        /// If this document has no path, an empty list is returned.
        /// </summary>
        public IOrderedQueryable<PublishedDocument> GetChildren(ContentDbContext context)
        {
            return context.ContentPublishedDocuments.AsNoTracking()
                .Where(d => d.Path!.StartsWith(this.Path! + '/') && d.PathSegmentsCount == this.PathSegmentsCount + 1)
                .OrderBy(d => d.SortKey).ThenBy(d => d.Name).ThenBy(d => d.Id);
        }

        /// <summary>
        /// Returns the descendance (children, grand-children, etc.) of this document.
        /// If this document has no path, an empty list is returned.
        /// </summary>
        public IQueryable<PublishedDocument> GetDescendance(ContentDbContext context)
        {
            return context.ContentPublishedDocuments.AsNoTracking()
                .Where(d => d.Path!.StartsWith(this.Path! + '/'));
        }
    }

    public class PublishedDocumentProperty
    {
        /// <summary>
        /// Name of the property.
        /// </summary>
        public virtual required string Name { get; set; }

        /// <summary>
        /// Combined settings of this property.
        /// </summary>
        public virtual Dictionary<string, object> Settings { get; set; } = [];

        /// <summary>
        /// Value of this property instance.
        /// </summary>
        public virtual string? Value { get; set; }

        /// <summary>
        /// Name of the data type.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual string? DataTypeName { get; set; }

        /// <summary>
        /// Name of the DisplayTemplate to use for rendering.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual string? DataTypeTemplate { get; set; }
    }
}
