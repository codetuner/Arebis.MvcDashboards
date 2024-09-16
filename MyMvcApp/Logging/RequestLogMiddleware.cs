using Microsoft.AspNetCore.Http;

namespace MyMvcApp.Logging
{
    public class RequestLogMiddleware : BaseRequestLogFilter
    {
        public RequestLogMiddleware(RequestDelegate next) : base(next)
        { }

        public override void PreInvoke(HttpContext context, RequestLogger requestLogger)
        {
            requestLogger.RequestStarted();
        }

        public override void PostInvoke(HttpContext context, RequestLogger requestLogger)
        {
            requestLogger.RequestEnded(context);
        }
    }
}
