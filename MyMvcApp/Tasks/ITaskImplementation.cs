namespace MyMvcApp.Tasks
{
    public interface ITaskImplementation
    {
        Task Execute(ITaskHost taskHost);
    }
}
