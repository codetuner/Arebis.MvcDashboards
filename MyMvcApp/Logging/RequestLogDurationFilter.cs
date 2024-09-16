using Microsoft.AspNetCore.Http;

namespace MyMvcApp.Logging
{
    public class RequestLogDurationFilter : BaseRequestLogFilter
    {
        public RequestLogDurationFilter(RequestDelegate next) : base(next)
        { }

        public override void PreInvoke(HttpContext context, RequestLogger requestLogger)
        { }

        public override void PostInvoke(HttpContext context, RequestLogger requestLogger)
        {
            if (requestLogger.DurationMs > 5000)
            {
                if (requestLogger.StoreLog == null) requestLogger.StoreLog = true;
                requestLogger.SetMessage(LogAspect.Timing.Name, false, null);
            }
        }
    }
}
