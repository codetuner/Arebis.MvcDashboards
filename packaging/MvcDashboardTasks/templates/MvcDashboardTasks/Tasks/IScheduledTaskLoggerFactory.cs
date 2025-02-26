using MyMvcApp.Data.Tasks;

namespace MyMvcApp.Tasks
{
    /// <summary>
    /// A factory to create task loggers.
    /// </summary>
    public interface IScheduledTaskLoggerFactory
    {
        /// <summary>
        /// Creates a logger for a scheduled task.
        /// </summary>
        IScheduledTaskLogger? CreateLogger(ScheduledTask forTask);
    }
}
