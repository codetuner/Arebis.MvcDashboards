namespace MyMvcApp.Tasks
{
    /// <summary>
    /// Base implementation for recurrent tasks.
    /// </summary>
    public abstract class BaseRecurrentTaskImplementation : BaseTaskImplementation
    {
        /// <summary>
        /// The recurrence interval to schedule the task.
        /// If null, the task is not rescheduled.
        /// </summary>
        [TaskArgumentAttribute]
        public TimeSpan? Recurrence { get; set; }

        /// <summary>
        /// Whether to skip or not schedules that are in the past.
        /// </summary>
        [TaskArgumentAttribute]
        public bool SkipPastSchedules { get; set; }

        public override Task Execute(ITaskHost host)
        {
            if (this.Recurrence.HasValue && this.Recurrence.Value.TotalSeconds >= 1)
            {
                var nextTime = host.UtcTimeToExecute.Add(this.Recurrence.Value);
                while(SkipPastSchedules && nextTime < DateTime.UtcNow)
                {
                    nextTime = nextTime.Add(this.Recurrence.Value);
                }
                host.RescheduleCurrentTask(nextTime);
            }

            return Task.CompletedTask;
        }
    }
}
