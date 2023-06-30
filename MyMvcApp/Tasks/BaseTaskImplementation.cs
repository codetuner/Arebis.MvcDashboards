namespace MyMvcApp.Tasks
{
    public abstract class BaseTaskImplementation : ITaskImplementation
    {
        public abstract Task Execute(ITaskHost taskHost);
    }
}