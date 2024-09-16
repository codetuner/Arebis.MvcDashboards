using Microsoft.AspNetCore.Http;
using MyMvcApp.Data.Logging;
using System.Collections.Generic;
using System.Linq;

namespace MyMvcApp.Logging
{
    public class RequestDoNotLogRuleFilter : BaseRequestLogFilter
    {
        private static List<LogActionRule>? doNotLogRules = null;

        public static void FlushDoNotLogRulesCache()
        {
            doNotLogRules = null;
        }

        public RequestDoNotLogRuleFilter(RequestDelegate next) : base(next)
        { }

        public override void PreInvoke(HttpContext context, RequestLogger requestLogger)
        { }

        public override void PostInvoke(HttpContext context, RequestLogger requestLogger)
        {
            if (doNotLogRules == null)
            {
                var newDoNotLogRules = new List<LogActionRule>();
                foreach (var rule in requestLogger.Context.LogActionRules.Where(r => r.IsActive && r.Action == LogAction.DoNotLog).ToList())
                {
                    newDoNotLogRules.Add(rule);
                }
                doNotLogRules = newDoNotLogRules;
            }

            var request = context.Request;
            var response = context.Response;
            if (doNotLogRules.Any(r => r.Matches(request, response, requestLogger.GetApplicationName(), requestLogger.GetAspectName(), requestLogger.GetType())))
            {
                requestLogger.DoNotLog();
            }
        }
    }
}
