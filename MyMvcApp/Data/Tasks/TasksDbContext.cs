using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data.Localize;

namespace MyMvcApp.Data.Tasks
{
    public class TasksDbContext : DbContext
    {
        public TasksDbContext(DbContextOptions<TasksDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TaskDefinition> TaskDefinitions => Set<TaskDefinition>();
        
        public DbSet<Task> Tasks => Set<Task>();
    }
}
