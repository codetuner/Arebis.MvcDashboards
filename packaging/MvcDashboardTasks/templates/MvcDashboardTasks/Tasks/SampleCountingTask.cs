using MyMvcApp.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyMvcApp.Tasks
{
    public class SampleCountingTask : BaseRecurrentTaskImplementation
    {
        [TaskArgument]
        public int? Count { get; set; }

        public override Task Execute(IScheduledTaskHost taskHost)
        {
            // Reschedule task if recurrence has been set:
            base.Execute(taskHost);

            // Read task arguments:
            var count = this.Count ?? 10;
            taskHost.WriteLine($"Counting to {count}");

            // Perform task:
            for(int i=1;i<=count;i++)
            {
                // Abort if cancellation was requested:
                if (taskHost.CancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Task was aborted.");
                    break;
                }
                // Loop implementation:
                Console.WriteLine($"{taskHost.CurrentTaskId} : {DateTime.Now} : {i}");
                taskHost.WriteLine($"{i}");
                Thread.Sleep(1000);
            }

            // Finalize task:
            taskHost.WriteLine("Finished counting");
            return Task.CompletedTask;
        }
    }
}
