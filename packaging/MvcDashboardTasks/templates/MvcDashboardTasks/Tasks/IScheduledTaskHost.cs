using System;
using System.Collections.Generic;
using System.Threading;

#nullable enable

namespace MyMvcApp.Tasks
{
    /// <summary>
    /// Represents the host running the tasks.
    /// </summary>
    public interface IScheduledTaskHost
    {
        /// <summary>
        /// Date/time the task was scheduled to execute.
        /// </summary>
        DateTime UtcTimeToExecute { get; }

        /// <summary>
        /// Cancellation token to cancel running this task.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Id of the currently running task.
        /// </summary>
        int CurrentTaskId { get; }

        /// <summary>
        /// All arguments of the currently running task.
        /// </summary>
        IReadOnlyDictionary<string, string?> CurrentTaskArguments { get; }

        /// <summary>
        /// Optional logger to log messages from the task.
        /// </summary>
        IScheduledTaskLogger? Logger { get; }

        /// <summary>
        /// Reschedules the currently running task to rerun now or on the given time.
        /// </summary>
        void RescheduleCurrentTask(DateTime? utcTimeToReschedule = null, IDictionary<string, string?>? additionalArguments = null);
        
        /// <summary>
        /// Writes a line to the task output.
        /// </summary>
        void WriteLine(string str);
    }
}
