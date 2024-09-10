namespace MyMvcApp.Logging
{
    public class LoggingBuilder
    {
        public LoggingBuilder(IApplicationBuilder application)
        {
            this.Application = application;
        }

        public IApplicationBuilder Application { get; set; }
    }
}
