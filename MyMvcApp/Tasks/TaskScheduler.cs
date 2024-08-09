using MyMvcApp.Data.Tasks;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace MyMvcApp.Tasks
{
    public class TaskScheduler : IHostedService
    {
        private readonly CancellationTokenSource mainLoopCancellationTokenSource = new CancellationTokenSource();
        private readonly TaskSchedulerMainLoop mainLoop;
        private Thread? mainLoopThread;

        public TaskScheduler(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<TaskScheduler> logger)
        {
            this.mainLoop = new TaskSchedulerMainLoop(serviceProvider, configuration, logger, mainLoopCancellationTokenSource);
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
        private readonly ILogger<TaskScheduler> logger;
        private readonly CancellationTokenSource mainLoopCancellationTokenSource;
        private readonly Dictionary<string, (ConcurrentQueue<int>, Thread)> queueThreads = new();
        private readonly ConcurrentDictionary<Thread, Thread> runningThreads = new();
        private readonly string? processRole;
        private readonly int schedulerMinimalDelayMs;
        private readonly int schedulerExtendedDelayMs;

        public TaskSchedulerMainLoop(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<TaskScheduler> logger, CancellationTokenSource mainLoopCancellationTokenSource)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;
            this.logger = logger;
            this.mainLoopCancellationTokenSource = mainLoopCancellationTokenSource;
            this.processRole = configuration.GetValue<string?>("Tasks:ProcessRole", null);
            this.schedulerMinimalDelayMs = configuration.GetValue<int>("Tasks:SchedulerInitialDelayMs", 2000);
            this.schedulerExtendedDelayMs = configuration.GetValue<int>("Tasks:SchedulerExtendedDelayMs", 3000);
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
                        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
                        var tasks = dbContext.Tasks
                            .Include(t => t.Definition)
                            .Where(t => t.Definition.ProcessRole == null || t.Definition.ProcessRole == processRole)
                            .Where(t => t.MachineName == null || t.MachineName == Environment.MachineName)
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
                        // Retrieve task entity:
                        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
                        var task = dbContext.Tasks
                            .Include(t => t.Definition)
                            .Single(t => t.Id == taskId);

                        // Flag task as started:
                        task.MachineName = Environment.MachineName;
                        task.UtcTimeStarted = DateTime.UtcNow;
                        task.OutputWriteLine($"=== {task.UtcTimeStarted:yyyy/MM/yy HH:mm:ss} Started");
                        dbContext.SaveChanges();

                        // Try to run task implementation:
                        if (task.Definition.ImplementationClass == null) break;
                        try
                        {
                            // Retrieve implementation type:
                            var implementationType = Type.GetType(task.Definition.ImplementationClass);
                            if (implementationType == null) throw new Exception($"Could not find implementation class \"{task.Definition.ImplementationClass}\". Check class name or specify ProcessRole of the task definition.");
                            var implementation = (ITaskImplementation)ActivatorUtilities.CreateInstance(scope.ServiceProvider, implementationType)!;

                            // Parse arguments:
                            var arguments = new Dictionary<string, string?>();
                            AddArguments(arguments, task.Definition.Arguments);
                            AddArguments(arguments, task.Arguments);
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
                            var result = taskHost.Execute(implementation);
                            result.Wait();
                            task.Succeeded = true;
                        }
                        catch (Exception ex)
                        {
                            task.OutputWriteLine(ex.ToString());
                            task.Succeeded = false;
                        }
                        finally
                        {
                            task.UtcTimeDone = DateTime.UtcNow;
                            task.OutputWriteLine($"=== {task.UtcTimeDone:yyyy/MM/yy HH:mm:ss} Ended");
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
                return DateTime.Parse(strvalue);
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
                return Convert.ChangeType(strvalue, propertyType, CultureInfo.InvariantCulture);
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

        private class TaskHost : ITaskHost
        {
            private readonly TasksDbContext dbContext;
            private readonly Data.Tasks.Task currentTaskEntity;
            private readonly IReadOnlyDictionary<string, string?> currentTaskArguments;
            private readonly CancellationTokenSource cancellationTokenSource;

            public TaskHost(TasksDbContext dbContext, Data.Tasks.Task currentTaskEntity, IReadOnlyDictionary<string, string?> currentTaskArguments, CancellationTokenSource cancellationTokenSource)
            {
                this.dbContext = dbContext;
                this.currentTaskEntity = currentTaskEntity;
                this.currentTaskArguments = currentTaskArguments;
                this.cancellationTokenSource = cancellationTokenSource;
            }

            public CancellationToken CancellationToken => this.cancellationTokenSource.Token;

            public DateTime UtcTimeToExecute => this.currentTaskEntity.UtcTimeToExecute;

            public int CurrentTaskId => this.currentTaskEntity.Id;

            public IReadOnlyDictionary<string, string?> CurrentTaskArguments => this.CurrentTaskArguments;

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
                var task = new Data.Tasks.Task();
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

            internal System.Threading.Tasks.Task Execute(ITaskImplementation implementation)
            {
                // Execute task:
                var result = implementation.Execute(this);
                return result;
            }
        }
    }
}
