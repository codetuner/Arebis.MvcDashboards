using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Security;

namespace MyMvcApp.Logging
{
    public class RequestLogExceptionFilter : BaseRequestLogFilter
    {
        public RequestLogExceptionFilter(RequestDelegate next) : base(next)
        { }

        public override void PreInvoke(HttpContext context, RequestLogger requestLogger)
        { }

        public override void PostInvoke(HttpContext context, RequestLogger requestLogger)
        {
            var ex = requestLogger.Exception;
            if (ex != null)
            {
                if (requestLogger.StoreLog == null) requestLogger.StoreLog = true;
                var aspect = (ex is SecurityException) ? LogAspect.Security : LogAspect.Error;
                requestLogger.SetException(aspect.Name, true, ex);
                requestLogger.WriteLine(ex.ToString());
            }
        }
    }
}
