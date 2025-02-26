using Microsoft.Extensions.Configuration;
using MyMvcApp.Data.Tasks;
using System;
using System.IO;

namespace MyMvcApp.Tasks
{
    public class FileScheduledTaskLoggerFactory : IScheduledTaskLoggerFactory
    {
        private readonly IConfiguration config;
        private readonly string? filePath;
        private readonly string fileName;

        public FileScheduledTaskLoggerFactory(IConfiguration config)
        {
            this.config = config;
            this.filePath = config["Tasks:FileLoggerPath"] == null ? null : Environment.ExpandEnvironmentVariables(config["Tasks:FileLoggerPath"]!);
            this.fileName = config["Tasks:FileLoggerName"] ?? "{timestamp}-{taskid}.log";
        }

        public IScheduledTaskLogger? CreateLogger(ScheduledTask forTask)
        {
            if (filePath != null)
            {
                var realFileName = fileName
                    .Replace("{timestamp}", DateTime.Now.ToString("yyyyMMdd-HHmmss"))
                    .Replace("{taskid}", forTask.Id.ToString());
                var logger = new StreamScheduledTaskLogger(File.Create(Path.Combine(AppContext.BaseDirectory, filePath, realFileName)));
                logger.WriteToLog($"Logging task {forTask.Id} - {forTask.Name ?? forTask.Definition.Name }");
                logger.WriteToLog("");
                return logger;
            }
            else
            {
                return null;
            }
        }
    }
}
