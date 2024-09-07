namespace MyMvcApp.Tasks
{
    /// <summary>
    /// Base implementation for recurrent tasks.
    /// </summary>
    public abstract class BaseRecurrentTaskImplementation : BaseScheduledTaskImplementation
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

        /// <summary>
        /// Whether the scheduling is based on local time. By default, scheduling is based on UTC time.
        /// I.e. when scheduling on UTC base, the exact hour of a daily task shifts with Daylight Savings Time.
        /// When sheduling on local time, it would run at the same hour.
        /// Note that when scheduling is based on local time, it uses the server's timzone.
        /// </summary>
        [TaskArgumentAttribute]
        public bool LocalTimeBase { get; set; }

        public override Task Execute(IScheduledTaskHost host)
        {
            if (this.Recurrence.HasValue && this.Recurrence.Value.TotalSeconds >= 1)
            {
                var nextTime = host.UtcTimeToExecute;
                if (LocalTimeBase) nextTime = nextTime.ToLocalTime();
                nextTime = nextTime.Add(this.Recurrence.Value);
                while(SkipPastSchedules && nextTime < DateTime.UtcNow)
                {
                    nextTime = nextTime.Add(this.Recurrence.Value);
                }
                if (LocalTimeBase) nextTime = nextTime.ToUniversalTime();
                host.RescheduleCurrentTask(nextTime);
            }

            return Task.CompletedTask;
        }
    }
}
