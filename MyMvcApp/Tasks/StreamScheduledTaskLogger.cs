using System.IO;

namespace MyMvcApp.Tasks
{
    /// <summary>
    /// A stream-based logger that logs messages from scheduled tasks.
    /// </summary>
    public class StreamScheduledTaskLogger : IScheduledTaskLogger
    {
        private readonly FileStream fileStream;
        private readonly StreamWriter streamWriter;

        public StreamScheduledTaskLogger(FileStream fileStream)
        {
            this.fileStream = fileStream;
            this.streamWriter = new StreamWriter(fileStream);
        }

        public void WriteToLog(string message)
        {
            this.streamWriter.WriteLine(message);
        }

        public virtual void Dispose()
        {
            this.streamWriter.Flush();
            this.streamWriter.Dispose();
            this.fileStream.Dispose();
        }
    }
}
