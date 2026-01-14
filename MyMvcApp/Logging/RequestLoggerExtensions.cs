using Microsoft.AspNetCore.Builder;

namespace MyMvcApp.Logging
{
    /// <summary>
    /// Extension methods for the Arebis request logger middleware.
    /// </summary>
    public static class RequestLoggerExtensions
    {
        /// <summary>
        /// Installs the Arebis request logger middleware.
        /// </summary>
        public static LoggingBuilder UseArebisRequestLog(this IApplicationBuilder builder)
        {
            return new LoggingBuilder(builder.UseMiddleware<RequestLogMiddleware>());
        }

        /// <summary>
        /// Apply DoNotLog rules.
        /// </summary>
        public static LoggingBuilder ApplyDoNotLogRule(this LoggingBuilder builder)
        {
            builder.Application.UseMiddleware<RequestDoNotLogRuleFilter>();
            return builder;
        }

        /// <summary>
        /// Log slow requests.
        /// </summary>
        public static LoggingBuilder LogSlowRequests(this LoggingBuilder builder)
        {
            builder.Application.UseMiddleware<RequestLogDurationFilter>();
            return builder;
        }

        /// <summary>
        /// Log requests issuing an exception.
        /// </summary>
        public static LoggingBuilder LogExceptions(this LoggingBuilder builder)
        {
            builder.Application.UseMiddleware<RequestLogExceptionFilter>();
            return builder;
        }

        /// <summary>
        /// Log requests with failure status codes (4xx or 5xx).
        /// </summary>
        public static LoggingBuilder LogStatus(this LoggingBuilder builder)
        {
            builder.Application.UseMiddleware<RequestLogStatusFilter>();
            return builder;
        }

        /// <summary>
        /// Log requests with Not Found (404) status.
        /// </summary>
        public static LoggingBuilder LogNotFounds(this LoggingBuilder builder)
        {
            builder.Application.UseMiddleware<RequestLogNotFoundFilter>();
            return builder;
        }
    }
}
