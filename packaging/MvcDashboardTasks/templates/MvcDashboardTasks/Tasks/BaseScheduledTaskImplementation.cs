namespace MyMvcApp.Tasks
{
    public abstract class BaseScheduledTaskImplementation : IScheduledTaskImplementation
    {
        public abstract Task Execute(IScheduledTaskHost taskHost);
    }
}