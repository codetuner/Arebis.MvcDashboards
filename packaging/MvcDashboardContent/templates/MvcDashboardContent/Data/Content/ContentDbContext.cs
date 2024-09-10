using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MyMvcApp.Data.Content
{
    public class ContentDbContext : DbContext
    {
        public ContentDbContext(DbContextOptions<ContentDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Property>()
            //    .Property(p => p.Settings)
            //    .HasConversion(
            //        v => ContentDbContext.ToString(v),
            //        s => ContentDbContext.ToDictionary(s),
            //        new ValueComparer<Dictionary<string, string>>(
            //            (v1, v2) => String.Equals(ContentDbContext.ToString(v1), ContentDbContext.ToString(v2)),
            //            v => ContentDbContext.ToString(v)!.GetHashCode(),
            //            v => v.ToDictionary(p => p.Key, p => p.Value)
            //        )
            //    );

            // EF8 does not yet support dictionaries to JSON.
            // Workaround, see: https://github.com/npgsql/efcore.pg/issues/1825#issuecomment-1668481365

            // Make sure times are UTC:
            modelBuilder.Entity<Document>().Property(p => p.CreatedOnUtc)
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => new DateTime(v.Ticks, DateTimeKind.Utc));

            // Make sure times are UTC:
            modelBuilder.Entity<Document>().Property(p => p.ModifiedOnUtc)
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => new DateTime(v.Ticks, DateTimeKind.Utc));

            // Make sure times are UTC:
            modelBuilder.Entity<Document>().Property(p => p.LastPublishedOnUtc)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                    v => v.HasValue ? new DateTime(v.Value.Ticks, DateTimeKind.Utc) : (DateTime?)null);

            // Make sure times are UTC:
            modelBuilder.Entity<Document>().Property(p => p.DeletedOnUtc)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                    v => v.HasValue ? new DateTime(v.Value.Ticks, DateTimeKind.Utc) : (DateTime?)null);

            var jsonType = "nvarchar(max)";

            modelBuilder.Entity<DataType>().Property(p => p.Settings)
                .HasColumnType(jsonType)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.General)),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions(JsonSerializerDefaults.General))!);

            modelBuilder.Entity<PropertyType>().Property(p => p.Settings)
                .HasColumnType(jsonType)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.General)),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions(JsonSerializerDefaults.General))!);

            modelBuilder.Entity<Property>().Property(p => p.Settings)
                .HasColumnType(jsonType)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.General)),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions(JsonSerializerDefaults.General))!);

            modelBuilder.Entity<PublishedDocument>().Property(p => p.Properties)
                .HasColumnType(jsonType)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.General)),
                    v => JsonSerializer.Deserialize<List<PublishedDocumentProperty>>(v, new JsonSerializerOptions(JsonSerializerDefaults.General))!);
        }

        public DbSet<Content.Property> ContentProperties { get; set; }

        public DbSet<Content.PropertyType> ContentPropertyTypes { get; set; }

        public DbSet<Content.DataType> ContentDataTypes { get; set; }

        public DbSet<Content.Document> ContentDocuments { get; set; }
        
        public DbSet<Content.DocumentType> ContentDocumentTypes { get; set; }

        public DbSet<Content.PublishedDocument> ContentPublishedDocuments { get; set; }

        public DbSet<Content.SecuredPath> ContentSecuredPaths { get; set; }

        public DbSet<Content.PathRedirection> ContentPathRedirections { get; set; }

        /*
        #region Conversion helpers

        static string? ToString(Dictionary<string, string>? data)
        {
            if (data == null) return null;
            var sb = new StringBuilder();
            foreach (var pair in data)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(WebUtility.UrlEncode(pair.Key));
                sb.Append('=');
                sb.Append(WebUtility.UrlEncode(pair.Value));
            }
            return sb.ToString();
        }

        static Dictionary<string, string>? ToDictionary(string? data)
        {
            if (data == null) return null;
            var result = new Dictionary<string, string>();
            foreach (var pair in data.Split('&').Where(p => p.Contains('=')))
            {
                var parts = pair.Split('=');
                result[WebUtility.UrlDecode(parts[0])] = WebUtility.UrlDecode(parts[1]);
            }
            return result;
        }

        #endregion
        */
    }
}
