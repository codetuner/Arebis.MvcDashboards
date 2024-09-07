using Microsoft.EntityFrameworkCore;

namespace MyMvcApp.Data.Tasks
{
    public class ScheduledTasksDbContext : DbContext
    {
        public ScheduledTasksDbContext(DbContextOptions<ScheduledTasksDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make sure times are UTC:
            modelBuilder.Entity<ScheduledTask>().Property(p => p.UtcTimeCreated)
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => new DateTime(v.Ticks, DateTimeKind.Utc));

            // Make sure times are UTC:
            modelBuilder.Entity<ScheduledTask>().Property(p => p.UtcTimeToExecute)
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => new DateTime(v.Ticks, DateTimeKind.Utc));

            // Make sure times are UTC:
            modelBuilder.Entity<ScheduledTask>().Property(p => p.UtcTimeStarted)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                    v => v.HasValue ? new DateTime(v.Value.Ticks, DateTimeKind.Utc) : (DateTime?)null);

            // Make sure times are UTC:
            modelBuilder.Entity<ScheduledTask>().Property(p => p.UtcTimeDone)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                    v => v.HasValue ? new DateTime(v.Value.Ticks, DateTimeKind.Utc) : (DateTime?)null);
        }

        public DbSet<ScheduledTaskDefinition> TaskDefinitions => Set<ScheduledTaskDefinition>();
        
        public DbSet<ScheduledTask> Tasks => Set<ScheduledTask>();
    }
}
