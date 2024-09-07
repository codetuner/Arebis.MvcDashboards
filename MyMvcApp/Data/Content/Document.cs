using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace MyMvcApp.Data.Content
{
    /// <summary>
    /// A content document holding properties.
    /// </summary>
    [Table(nameof(Document), Schema = "content")]
    public class Document
    {
        #region Fields
        private string? path;
        #endregion

        /// <summary>
        /// Identifier of the document.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        /// <summary>
        /// Name of the document.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual required string Name { get; set; }

        [NotMapped]
        public string State
        {
            get
            {
                if (this.DeletedOnUtc != null) return "deleted";
                else if (this.LastPublishedOnUtc == null) return "new";
                else if (this.IsLatestPublished) return "uptodate";
                else return "outdated";
            }
        }
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
        /// <remarks>Automatically recalculates the PathSegmentsCount.</remarks>
        [MaxLength(2000)]
        [BackingField(nameof(path))]
        public virtual string? Path
        {
            get
            {
                return this.path;
            }
            set
            {
                value = value?.Trim();
                if (value != null && value.StartsWith("/"))
                {
                    while (value.Length > 1 && value.EndsWith("/")) value = value[0..^1]; // Trim trailing slash.
                    if (value.Length == 1)
                        this.PathSegmentsCount = 0;
                    else
                        this.PathSegmentsCount = value.Count(c => c == '/');
                }
                else
                {
                    this.PathSegmentsCount = null;
                }

                this.path = value;
            }
        }

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
        /// Type of the document.
        /// </summary>
        public virtual int TypeId { get; set; }

        /// <summary>
        /// Type of the document.
        /// </summary>
        [ForeignKey(nameof(TypeId))]
        public virtual DocumentType? Type { get; set; }

        /// <summary>
        /// Properties of this document.
        /// </summary>
        [InverseProperty(nameof(Property.Document))]
        public virtual List<Property>? Properties { get; set; }

        /// <summary>
        /// [NotMapped] Returns the property with the given name.
        /// Requires the Properties collection to be loaded.
        /// </summary>
        /// <remarks>
        /// If no property is found with that name, one is created with an empty type definition.
        /// This to cover the case where a document is created and later on property types are added
        /// to its document type or one of its base types.
        /// </remarks>
        [NotMapped]
        public Property this[string name]
        {
            get
            {
                return this.Properties?.FirstOrDefault(p => p.Type?.Name == name)
                    ?? new Property() { Document = this, Type = new PropertyType() { Name = name, DataType = null!, DocumentType = null! } };
            }
        }

        /// <summary>
        /// Internal notes.
        /// </summary>
        public virtual string? Notes { get; set; }

        /// <summary>
        /// Whether to publish the document on every update.
        /// </summary>
        public virtual bool AutoPublish { get; set; }

        /// <summary>
        /// Whether the latest version of this document is also the published version.
        /// </summary>
        public virtual bool IsLatestPublished { get; set; }

        /// <summary>
        /// UTC date/time this document was created.
        /// </summary>
        public virtual DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// User this document was created by.
        /// </summary>
        [MaxLength(256)]
        public virtual string? CreatedBy { get; set; }

        /// <summary>
        /// UTC date/time this document was last modified.
        /// </summary>
        public virtual DateTime ModifiedOnUtc { get; set; }

        /// <summary>
        /// User this document was last modified by.
        /// </summary>
        [MaxLength(256)]
        public virtual string? ModifiedBy { get; set; }

        /// <summary>
        /// UTC date/time this document was last published.
        /// </summary>
        public virtual DateTime? LastPublishedOnUtc { get; set; }

        /// <summary>
        /// User this document was last published by.
        /// </summary>
        [MaxLength(256)]
        public virtual string? LastPublishedBy { get; set; }

        /// <summary>
        /// UTC date/time this document was deleted.
        /// </summary>
        public virtual DateTime? DeletedOnUtc { get; set; }

        /// <summary>
        /// User this document was deleted by.
        /// </summary>
        [MaxLength(256)]
        public virtual string? DeletedBy { get; set; }

        /// <summary>
        /// Saves the current document.
        /// </summary>
        public void Save(ContentDbContext context, IIdentity? byUser)
        {
            // Attach as Updated or Added to context:
            var entry = context.Update(this);

            // A newer version has been saved:
            this.IsLatestPublished = false;

            // Mark modified:
            this.ModifiedBy = byUser?.Name;
            this.ModifiedOnUtc = DateTime.UtcNow;
            entry.Property(nameof(this.LastPublishedBy)).IsModified = false;
            entry.Property(nameof(this.LastPublishedOnUtc)).IsModified = false;

            // Mark not deleted:
            this.DeletedBy = null;
            this.DeletedOnUtc = null;

            if (this.Id == 0)
            {
                // Mark created:
                this.CreatedBy = this.ModifiedBy;
                this.CreatedOnUtc = this.ModifiedOnUtc;
            }
            else
            {
                entry.Property(nameof(CreatedBy)).IsModified = false;
                entry.Property(nameof(CreatedOnUtc)).IsModified = false;
            }
        }

        /// <summary>
        /// Publishes or updates the publication of this document.
        /// </summary>
        public async Task<PublishedDocument> PublishAsync(ContentDbContext context, IIdentity? byUser, CancellationToken ct = default)
        {
            // Ensure required data is loaded if no eager loading was done and lazy loading is not enabled:
            if (this.Id == 0) throw new InvalidOperationException("A document needs to be saved before it can be published.");
            if (this.Type == null) await context.Entry(this).Reference(e => e.Type).LoadAsync(ct);
            if (this.Type == null) throw new InvalidOperationException("A document needs to have a type in order to be published.");
            if (this.Properties == null) await context.Entry(this).Collection(e => e.Properties!).LoadAsync(ct);
            if (this.Properties == null) this.Properties = new();
            foreach(var item in this.Properties)
            {
                if (item.Type == null) await context.Entry(item).Reference(e => e.Type).LoadAsync(ct);
                if (item.Type == null) continue;
                if (item.Type.DataType == null) await context.Entry(item.Type).Reference(e => e.DataType).LoadAsync(ct);
            }

            // Get or create published document instance:
            var published = await context.ContentPublishedDocuments.SingleOrDefaultAsync(d => d.DocumentId == this.Id, ct);
            if (published == null)
            {
                published = new PublishedDocument() { Document = this, DocumentTypeName = this.Type.Name, Name = this.Name, Version = 0 };
                context.ContentPublishedDocuments.Add(published);
            }

            // Update or publish changes:
            published.Version++;
            published.Name = this.Name;
            published.Culture = this.Culture;
            published.Path = this.Path;
            published.PathSegmentsCount = this.PathSegmentsCount;
            published.SortKey = this.SortKey;
            published.DocumentTypeName = this.Type.Name;
            published.ViewName = this.Type.ViewName;
            published.Properties = this.Properties
                .Where(p => p.Type != null && p.Type.DataType != null)
                .Select(p => new PublishedDocumentProperty()
                {
                    Name = p.Type!.Name,
                    Value = p.Value,
                    Settings = p.CombinedSettings,
                    DataTypeName = p.Type.DataType!.Name,
                    DataTypeTemplate = p.Type.DataType.Template
                }).ToList();

            // Mark published:
            this.IsLatestPublished = true;
            this.LastPublishedBy = byUser?.Name;
            this.LastPublishedOnUtc = DateTime.UtcNow;

            // Mark modified:
            this.ModifiedBy = byUser?.Name;
            this.ModifiedOnUtc = DateTime.UtcNow;

            // Return published document for preview:
            return published;
        }

        /// <summary>
        /// Withdraw this document from publication.
        /// </summary>
        public async Task UnpublishAsync(ContentDbContext context, IIdentity? byUser, CancellationToken ct = default)
        {
            // Delete published version:
            await context.ContentPublishedDocuments.Where(d => d.DocumentId == this.Id).ExecuteDeleteAsync(ct);

            // Mark published:
            this.IsLatestPublished = false;
            this.LastPublishedBy = null;
            this.LastPublishedOnUtc = null;

            // Mark modified:
            this.ModifiedBy = byUser?.Name;
            this.ModifiedOnUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft-deletes the current document.
        /// </summary>
        public async Task DeleteAsync(ContentDbContext context, IIdentity? byUser, CancellationToken ct = default)
        {
            // Deleting implies unpublishing:
            await context.ContentPublishedDocuments.Where(d => d.DocumentId == this.Id).ExecuteDeleteAsync(ct);
            this.IsLatestPublished = false;
            this.LastPublishedBy = null;
            this.LastPublishedOnUtc = null;

            // Soft delete:
            this.DeletedBy = byUser?.Name;
            this.DeletedOnUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Restores after soft-delete the current document.
        /// </summary>
        public void Restore(ContentDbContext context, IIdentity? byUser)
        {
            // Restore:
            this.DeletedBy = null;
            this.DeletedOnUtc = null;
        }
    }
}
