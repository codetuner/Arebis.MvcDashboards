using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MyMvcApp.Data.Logging
{
    public class LoggingDbContext : DbContext
    {
        public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var jsOptions = new JsonSerializerOptions() { };

            // Make sure times are UTC:
            modelBuilder.Entity<RequestLog>().Property(p => p.Timestamp)
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => new DateTime(v.Ticks, DateTimeKind.Utc));

            // Store RequestLog Data as JSON:
            modelBuilder.Entity<RequestLog>()
                .Property(e => e.Data)
                .HasConversion(
                v => JsonSerializer.Serialize(v, jsOptions),
                s => JsonSerializer.Deserialize<Dictionary<string, string>>(s, jsOptions) ?? new(),
                new ValueComparer<Dictionary<string, string>>(
                    (v1, v2) => String.Equals(JsonSerializer.Serialize(v1, jsOptions), JsonSerializer.Serialize(v2, jsOptions)),
                    v => JsonSerializer.Serialize(v, jsOptions).GetHashCode(),
                    v => v.ToDictionary(p => p.Key, p => p.Value)
                )
            );

            // Store RequestLog Request as JSON:
            modelBuilder.Entity<RequestLog>()
                .Property(e => e.Request)
                .HasConversion(
                j => JsonSerializer.Serialize(j, jsOptions),
                s => JsonSerializer.Deserialize<Dictionary<string, string>>(s, jsOptions) ?? new(),
                new ValueComparer<Dictionary<string, string>>(
                    (v1, v2) => String.Equals(JsonSerializer.Serialize(v1, jsOptions), JsonSerializer.Serialize(v2, jsOptions)),
                    v => JsonSerializer.Serialize(v, jsOptions).GetHashCode(),
                    v => v.ToDictionary(p => p.Key, p => p.Value)
                )
            );
        }

        public DbSet<LogActionRule> LogActionRules => Set<LogActionRule>();

        public DbSet<RequestLog> RequestLogs => Set<RequestLog>();
    }
}
