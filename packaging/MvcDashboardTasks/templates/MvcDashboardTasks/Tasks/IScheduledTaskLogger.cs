using System;

namespace MyMvcApp.Tasks
{
    /// <summary>
    /// A logger that logs messages from scheduled tasks.
    /// </summary>
    public interface IScheduledTaskLogger : IDisposable
    {
        /// <summary>
        /// Writes a message to a task log.
        /// </summary>
        void WriteToLog(string message);
    }
}
