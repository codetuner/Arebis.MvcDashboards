using Microsoft.AspNetCore.Http;

namespace MyMvcApp.Logging
{
    public class RequestLogStatusFilter : BaseRequestLogFilter
    {
        public RequestLogStatusFilter(RequestDelegate next) : base(next)
        { }

        public override void PreInvoke(HttpContext context, RequestLogger requestLogger)
        { }

        public override void PostInvoke(HttpContext context, RequestLogger requestLogger)
        {
            // Log 4xx and 5xx responses as errors (except 404):
            if (context.Response.StatusCode != 404 && (context.Response.StatusCode / 100 == 4 || context.Response.StatusCode / 100 == 5))
            {
                if (requestLogger.StoreLog == null) requestLogger.StoreLog = true;
                requestLogger.SetMessage(LogAspect.Error.Name, false, $"Response status {context.Response.StatusCode}");
            }
        }
    }
}
