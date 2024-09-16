using Microsoft.AspNetCore.Http;

namespace MyMvcApp.Logging
{
    public class RequestLogNotFoundFilter : BaseRequestLogFilter
    {
        public RequestLogNotFoundFilter(RequestDelegate next) : base(next)
        { }

        public override void PreInvoke(HttpContext context, RequestLogger requestLogger)
        { }

        public override void PostInvoke(HttpContext context, RequestLogger requestLogger)
        {
            if (context.Response.StatusCode == 404)
            {
                if (requestLogger.StoreLog == null) requestLogger.StoreLog = true;
                requestLogger.SetMessage(LogAspect.NotFound.Name, false, null);
            }
        }
    }
}
