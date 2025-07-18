using MyMvcApp.Data.Tasks;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

#nullable enable

namespace MyMvcApp.Tasks
{
    public class TaskScheduler : IHostedService
    {
        private readonly CancellationTokenSource mainLoopCancellationTokenSource = new CancellationTokenSource();
        private readonly TaskSchedulerMainLoop mainLoop;
        private Thread? mainLoopThread;

        public TaskScheduler(IServiceProvider serviceProvider, IConfiguration configuration, IWebHostEnvironment environment, ILogger<TaskScheduler> logger, IScheduledTaskLoggerFactory? taskLoggerFactory)
        {
            this.mainLoop = new TaskSchedulerMainLoop(serviceProvider, configuration, environment, logger, taskLoggerFactory, mainLoopCancellationTokenSource);
        }

        public System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            // Start a main loop thread:
            mainLoopThread = new Thread(new ThreadStart(mainLoop.Run));
            mainLoopThread.Start();

            // Done:
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            // Request to abort the main loop:
            this.mainLoopCancellationTokenSource.Cancel();

            // Wait for the main loop thread to end (which also waits for all running tasks to end):
            mainLoopThread?.Join();

            // Done:
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }

    public class TaskSchedulerMainLoop
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<TaskScheduler> logger;
        private readonly IScheduledTaskLoggerFactory? taskLoggerFactory;
        private readonly CancellationTokenSource mainLoopCancellationTokenSource;
        private readonly Dictionary<string, (ConcurrentQueue<int>, Thread)> queueThreads = new();
        private readonly ConcurrentDictionary<Thread, Thread> runningThreads = new();
        private readonly string? processRole;
        private readonly int schedulerMinimalDelayMs;
        private readonly int schedulerExtendedDelayMs;

        public TaskSchedulerMainLoop(IServiceProvider serviceProvider, IConfiguration configuration, IWebHostEnvironment environment, ILogger<TaskScheduler> logger, IScheduledTaskLoggerFactory? taskLoggerFactory, CancellationTokenSource mainLoopCancellationTokenSource)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;
            this.environment = environment;
            this.logger = logger;
            this.taskLoggerFactory = taskLoggerFactory;
            this.mainLoopCancellationTokenSource = mainLoopCancellationTokenSource;
            this.processRole = configuration.GetValue<string?>("Tasks:ProcessRole", null);
            this.schedulerMinimalDelayMs = configuration.GetValue<int>("Tasks:SchedulerInitialDelayMs", 2000);
            this.schedulerExtendedDelayMs = configuration.GetValue<int>("Tasks:SchedulerExtendedDelayMs", 23000);
        }

        public void Run()
        {
            var mainLoopCancellationToken = mainLoopCancellationTokenSource.Token;
            while (true)
            {
                // Wait a little before requerying the database for tasks:
                if (mainLoopCancellationToken.WaitHandle.WaitOne(schedulerMinimalDelayMs))
                {
                    break;
                }

                try
                {
                    using (IServiceScope scope = serviceProvider.CreateScope())
                    {
                        // Retrieve tasks to execute:
                        var dbContext = scope.ServiceProvider.GetRequiredService<ScheduledTasksDbContext>();
                        var tasks = dbContext.Tasks
                            .Include(t => t.Definition)
                            .Where(t => t.Definition.ProcessRole == null || t.Definition.ProcessRole == processRole)
                            .Where(t => t.MachineNameToRunOn == null || t.MachineNameToRunOn == Environment.MachineName)
                            .Where(t => t.Definition.IsActive == true)
                            .Where(t => t.UtcTimeToExecute <= DateTime.UtcNow && t.UtcTimeStarted == null)
                            .OrderBy(t => t.UtcTimeToExecute)
                            .Take(20)
                            .ToList();

                        // If no tasks, take longer nap:
                        if (!tasks.Any()) if (mainLoopCancellationToken.WaitHandle.WaitOne(schedulerExtendedDelayMs)) break; else continue;

                        // Post tasks on respective queues:
                        foreach (var task in tasks)
                        {
                            // If no queue, execute immediately on dedicated thread:
                            if (task.QueueName == null)
                            {
                                var thread = new Thread(this.TaskRun);
                                thread.Start(((String?)null, new ConcurrentQueue<int>(new int[] { task.Id })));
                                runningThreads[thread] = thread;
                            }
                            else
                            {
                                // Create QueueThread if missing:
                                lock (queueThreads)
                                {
                                    if (!queueThreads.TryGetValue(task.QueueName, out (ConcurrentQueue<int>, Thread) queueThread))
                                    {
                                        var thread = new Thread(this.TaskRun);
                                        queueThread = new(new ConcurrentQueue<int>(), thread);
                                        queueThreads[task.QueueName] = queueThread;
                                    }

                                    // Post task on queue:
                                    if (!queueThread.Item1.Any(id => id == task.Id))
                                    {
                                        // Enqueue task:
                                        queueThread.Item1.Enqueue(task.Id);

                                        // Make sure thread is started:
                                        if (!queueThread.Item2.IsAlive)
                                        {
                                            queueThread.Item2.Start((task.QueueName, queueThread.Item1));
                                        }
                                    }
                                }
                            }
                        }

                        // Remove idle queues (except default one):
                        var idleQueues = queueThreads.Where(q => q.Value.Item1.IsEmpty && q.Key != "default" && q.Value.Item2.IsAlive == false).Select(q => q.Key).ToList();
                        foreach (var idleQueue in idleQueues)
                        {
                            queueThreads.Remove(idleQueue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex, "TaskSchedulerMainLoop failure.");
                }
            }

            // Wait for all running threads to finish:
            foreach (var thread in runningThreads)
            {
                thread.Key.Join();
            }
        }

        private void TaskRun(object? argsObject)
        {
            var args = ((string?, ConcurrentQueue<int>))argsObject!;
            var queueName = args.Item1;
            var queue = args.Item2;

            while (queue.TryDequeue(out int taskId))
            {
                var taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(mainLoopCancellationTokenSource.Token);
                try
                {
                    using (IServiceScope scope = serviceProvider.CreateScope())
                    {
                        // Retrieve non-running task entity:
                        var dbContext = scope.ServiceProvider.GetRequiredService<ScheduledTasksDbContext>();
                        var task = dbContext.Tasks
                            .Include(t => t.Definition)
                            .Where(t => t.UtcTimeStarted == null)
                            .SingleOrDefault(t => t.Id == taskId);
                        if (task == null) continue;

                        // Flag task as started:
                        task.MachineNameRanOn = Environment.MachineName;
                        task.UtcTimeStarted = DateTime.UtcNow;
                        task.OutputWriteLine($"=== {task.UtcTimeStarted:yyyy/MM/dd HH:mm:ss} Started");
                        dbContext.SaveChanges();

                        // Try to run task implementation:
                        try
                        {
                            if (task.Definition.ImplementationClass != null)
                            {
                                // Retrieve implementation type:
                                var implementationType = Type.GetType(task.Definition.ImplementationClass);
                                if (implementationType == null) throw new Exception($"Could not find implementation class \"{task.Definition.ImplementationClass}\". Check class name or specify ProcessRole of the task definition.");
                                var implementation = (IScheduledTaskImplementation)ActivatorUtilities.CreateInstance(scope.ServiceProvider, implementationType)!;

                                // Parse arguments:
                                var arguments = new Dictionary<string, string?>();
                                AddArguments(arguments, task.Definition.Arguments);
                                AddArguments(arguments, task.Arguments);

                                if (!this.environment.IsProduction() && arguments.Any(a => "ProductionOnly".Equals(a.Key) && a.Value != null && a.Value.Equals("true", StringComparison.OrdinalIgnoreCase)))
                                {
                                    task.OutputWriteLine($"Task is set to run only in production mode, but the application is not running in production mode. Skipping task.");
                                }
                                else
                                {
                                    // Set task arguments on implementation properties:
                                    foreach (var prop in implementation.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
                                    {
                                        var attr = (TaskArgumentAttribute?)prop.GetCustomAttribute(typeof(TaskArgumentAttribute));
                                        if (attr != null)
                                        {
                                            if (arguments.TryGetValue(attr.Name ?? prop.Name, out string? strvalue))
                                            {
                                                prop.SetValue(implementation, ConvertValue(strvalue, prop.PropertyType));
                                            }
                                        }
                                    }

                                    // Execute task:
                                    var taskHost = new TaskHost(dbContext, task, arguments, taskCancellationTokenSource);
                                    taskHost.Logger = this.taskLoggerFactory?.CreateLogger(task);
                                    try
                                    {
                                        var result = taskHost.Execute(implementation);
                                        result.Wait();
                                    }
                                    finally
                                    {
                                        taskHost.Logger?.Dispose();
                                    }
                                    task.Succeeded = true;
                                }
                            }
                            else
                            {
                                task.OutputWriteLine($"Task definition has no implementation class to run.");
                                task.Succeeded = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            task.OutputWriteLine(ex.ToString());
                            task.Succeeded = false;
                        }
                        finally
                        {
                            task.UtcTimeDone = DateTime.UtcNow;
                            task.OutputWriteLine($"=== {task.UtcTimeDone:yyyy/MM/dd HH:mm:ss} Ended");
                        }
                        dbContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex, $"TaskSchedulerMainLoop failure to run task {taskId}.");
                }

                // Check mainLoopCancellationToken before dequeueing next task:
                if (mainLoopCancellationTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }
            }

            lock (queueThreads)
            {
                if (queue.IsEmpty)
                {
                    // Remove current thread from running threads:
                    runningThreads.Remove(Thread.CurrentThread, out Thread? t);

                    // Remove queue:
                    if (queueName != null) queueThreads.Remove(queueName);

                    // End thread:
                    return;
                }
            }
        }

        private static object? ConvertValue(string? strvalue, Type propertyType)
        {
            if (String.IsNullOrEmpty(strvalue))
            {
                if (propertyType == typeof(int[]))
                    return Array.Empty<int>();
                else if (propertyType == typeof(string[]))
                    return Array.Empty<string>();
                else if (propertyType == typeof(TimeSpan))
                    return TimeSpan.Zero;
                else
                    return null;
            }
            else if (propertyType == typeof(int[]))
            {
                return strvalue.Split(',').Select(s => Int32.Parse(s)).ToArray();
            }
            else if (propertyType == typeof(string[]))
            {
                return strvalue.Split(',');
            }
            else if (propertyType == typeof(TimeSpan) || (propertyType == typeof(TimeSpan?)))
            {
                return TimeSpan.Parse(strvalue);
            }
            else if (propertyType == typeof(Int32) || propertyType == typeof(Int32?))
            {
                return Int32.Parse(strvalue);
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                if (strvalue.EndsWith('Z'))
                    return DateTime.Parse(strvalue).ToUniversalTime();
                else
                    return DateTime.Parse(strvalue).ToUniversalTime().ToLocalTime();
            }
            else if (propertyType == typeof(DateTimeOffset) || propertyType == typeof(DateTimeOffset?))
            {
                return DateTimeOffset.Parse(strvalue);
            }
            else if (propertyType == typeof(DateOnly) || propertyType == typeof(DateOnly?))
            {
                return DateOnly.Parse(strvalue);
            }
            else if (propertyType == typeof(TimeOnly) || propertyType == typeof(TimeOnly?))
            {
                return TimeOnly.Parse(strvalue);
            }
            else
            {
                return Convert.ChangeType(strvalue, Nullable.GetUnderlyingType(propertyType) ?? propertyType, CultureInfo.InvariantCulture);
            }
        }

        private static void AddArguments(IDictionary<string, string?> dict, string? argString)
        {
            if (!String.IsNullOrWhiteSpace(argString))
            {
                foreach (var line in argString.Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.StartsWith(';')) continue;
                    var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        dict[parts[0]] = parts[1];
                    }
                    else
                    {
                        dict[parts[0]] = null;
                    }
                }
            }
        }

        private class TaskHost : IScheduledTaskHost
        {
            private readonly ScheduledTasksDbContext dbContext;
            private readonly Data.Tasks.ScheduledTask currentTaskEntity;
            private readonly IReadOnlyDictionary<string, string?> currentTaskArguments;
            private readonly CancellationTokenSource cancellationTokenSource;

            public TaskHost(ScheduledTasksDbContext dbContext, Data.Tasks.ScheduledTask currentTaskEntity, IReadOnlyDictionary<string, string?> currentTaskArguments, CancellationTokenSource cancellationTokenSource)
            {
                this.dbContext = dbContext;
                this.currentTaskEntity = currentTaskEntity;
                this.currentTaskArguments = currentTaskArguments;
                this.cancellationTokenSource = cancellationTokenSource;
            }

            public CancellationToken CancellationToken => this.cancellationTokenSource.Token;

            public DateTime UtcTimeToExecute => this.currentTaskEntity.UtcTimeToExecute;

            public int CurrentTaskId => this.currentTaskEntity.Id;

            public ScheduledTask CurrentTaskEntity => this.currentTaskEntity;

            public IReadOnlyDictionary<string, string?> CurrentTaskArguments => this.CurrentTaskArguments;

            public IScheduledTaskLogger? Logger { get; set; }

            public void RescheduleCurrentTask(DateTime? utcTimeToReschedule = null, IDictionary<string, string?>? additionalArguments = null)
            {
                // Build arguments of next scheduled instance:
                var nextTaskArguments = new Dictionary<string, string?>();
                AddArguments(nextTaskArguments, currentTaskEntity.Arguments);
                if (additionalArguments != null)
                {
                    foreach (var pair in additionalArguments)
                    {
                        nextTaskArguments[pair.Key] = pair.Value;
                    }
                }
                nextTaskArguments["PreviousTaskId"] = currentTaskEntity.Id.ToString();

                // Create next scheduled instance:
                var task = new Data.Tasks.ScheduledTask();
                task.Name = currentTaskEntity.Name;
                task.DefinitionId = currentTaskEntity.DefinitionId;
                task.QueueName = currentTaskEntity.QueueName;
                task.Arguments = String.Join("\r\n", nextTaskArguments.Select(a => a.Key + "=" + a.Value));
                task.UtcTimeToExecute = utcTimeToReschedule ?? DateTime.UtcNow;
                dbContext.Tasks.Add(task);
                dbContext.SaveChanges();
            }

            public void WriteLine(string str)
            {
                currentTaskEntity.OutputWriteLine(str);
            }

            internal System.Threading.Tasks.Task Execute(IScheduledTaskImplementation implementation)
            {
                // Execute task:
                var result = implementation.Execute(this);
                return result;
            }
        }
    }
}
