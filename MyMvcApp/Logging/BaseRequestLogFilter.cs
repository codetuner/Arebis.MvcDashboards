namespace MyMvcApp.Logging
{
    public abstract class BaseRequestLogFilter
    {
        private readonly RequestDelegate _next;

        public BaseRequestLogFilter(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, RequestLogger requestLogger)
        {
            PreInvoke(context, requestLogger);
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                requestLogger.Exception = ex;
                throw;
            }
            finally
            {
                PostInvoke(context, requestLogger);
            }
        }

        public abstract void PreInvoke(HttpContext context, RequestLogger requestLogger);

        public abstract void PostInvoke(HttpContext context, RequestLogger requestLogger);
    }
}
