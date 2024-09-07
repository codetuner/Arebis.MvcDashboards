using Microsoft.EntityFrameworkCore;

namespace MyMvcApp.Tasks
{
    public class TasksFlushTask : BaseRecurrentTaskImplementation
    {
        private readonly Data.Tasks.ScheduledTasksDbContext context;

        public TasksFlushTask(Data.Tasks.ScheduledTasksDbContext context)
        {
            this.context = context;

            this.Recurrence = TimeSpan.FromDays(1);
            this.SuccessRetentionDays = 30;
            this.FailureRetentionDays = 60;
        }

        /// <summary>
        /// Number of days to keep succeeded tasks.
        /// </summary>
        [TaskArgumentAttribute]
        public int SuccessRetentionDays { get; set; }

        /// <summary>
        /// Number of days to keep failed tasks.
        /// </summary>
        [TaskArgumentAttribute]
        public int FailureRetentionDays { get; set; }

        public override Task Execute(IScheduledTaskHost host)
        {
            // Reschedule task:
            base.Execute(host);

            // Delete succeeded tasks:
            context.Tasks.Where(t => t.Succeeded == true && t.UtcTimeDone.HasValue && t.UtcTimeDone < DateTime.UtcNow.AddDays(-SuccessRetentionDays))
                .ExecuteDelete();

            // Delete failed tasks:
            context.Tasks.Where(t => t.Succeeded == false && t.UtcTimeDone.HasValue && t.UtcTimeDone < DateTime.UtcNow.AddDays(-FailureRetentionDays))
                .ExecuteDelete();

            return Task.CompletedTask;
        }
    }
}
