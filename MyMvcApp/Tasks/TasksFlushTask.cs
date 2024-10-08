﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            this.FailureRetentionDays = 90;
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
            int c1 = context.Tasks.Where(t => t.Succeeded == true && t.UtcTimeDone.HasValue && t.UtcTimeDone < DateTime.UtcNow.AddDays(-SuccessRetentionDays))
                .ExecuteDelete();

            // Delete failed tasks:
            int c2 = context.Tasks.Where(t => t.Succeeded == false && t.UtcTimeDone.HasValue && t.UtcTimeDone < DateTime.UtcNow.AddDays(-FailureRetentionDays))
                .ExecuteDelete();

            host.WriteLine($"{c1} succeeded tasks deleted and {c2} failed tasks deleted.");

            return Task.CompletedTask;
        }
    }
}
