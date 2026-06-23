using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MyMvcApp.Data.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

#nullable enable

namespace MyMvcApp.Logging
{
    public class RequestLogger
    {
        private RequestLog record = null!;
        private Stopwatch? stopwatch;
        private StringBuilder detailsBuilder = new();
        private bool doNotLog = false;
        private readonly string? applicationName;

        public RequestLogger(IConfiguration configuration, LoggingDbContext context)
        {
            this.Context = context;
            this.Configuration = configuration;
            this.applicationName = Configuration.GetValue<string?>("LoggingApplicationName", Configuration.GetValue<string>("ApplicationName"));
        }

        public LoggingDbContext Context { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public bool? StoreLog { get; set; }

        public long DurationMs => this.stopwatch?.ElapsedMilliseconds ?? 0L;

        public Exception? Exception { get; set; }

        public void RequestStarted(HttpContext httpContext)
        {
            this.record = NewRecord(httpContext);
            this.record.Timestamp = DateTime.UtcNow;
            this.stopwatch = Stopwatch.StartNew();
        }

        public void RequestEnded(HttpContext httpContext)
        {
            if (this.doNotLog == false && this.StoreLog == true)
            {
                // Add information:
                this.record.Details = this.detailsBuilder.ToString();
                this.record.DurationMs = this.stopwatch?.ElapsedMilliseconds ?? 0L;
                this.record.TraceIdentifier = Activity.Current?.Id ?? httpContext.TraceIdentifier;

                // Additional information:
                foreach (var pair in httpContext.Items.Where(pair => pair.Value != null))
                {
                    this.record.Request["Items: " + pair.Key] = pair.Value?.ToString() ?? "(none)";
                }

                // Add response information:
                this.record.StatusCode = httpContext.Response.StatusCode;

                // Store the record:
                this.Context.RequestLogs.Add(this.record);
                this.Context.SaveChanges();
            }
        }

        public RequestLog NewRecord(HttpContext httpContext)
        {
            // Create a new record:
            var record = new RequestLog();

            // Add request information:
            record.Host = Environment.MachineName;
            record.ApplicationName = GetApplicationName();
            record.Method = httpContext.Request.Method;
            record.Url = TrimLength(httpContext.Request.Path.ToString(), 2000);
            record.User = httpContext.User?.Identity?.Name;
            record.Request["QueryString"] = httpContext.Request.QueryString.Value ?? String.Empty;
            record.Request["Scheme"] = httpContext.Request.Scheme;
            foreach (var pair in httpContext.Request.Headers)
            {
                record.Request["Header: " + pair.Key] = pair.Value!;
            }
            if (httpContext.Request.HasFormContentType)
            {
                foreach (var pair in httpContext.Request.Form)
                {
                    record.Request["Form: " + pair.Key] = pair.Value!;
                }
            }

            // Return the record:
            return record;
        }

        [return: NotNullIfNotNull(nameof(str))]
        private static string? TrimLength(string? str, int len)
        {
            if (str == null) return null;
            return (str.Length <= len) ? str : str[..len];
        }

        public void DoNotLog()
        {
            this.doNotLog = true;
        }

        public string? GetAspectName()
        {
            return this.record.AspectName;
        }

        public new string? GetType()
        {
            return this.record.Type;
        }

        public RequestLogger SetException(string aspectName, bool @override, Exception ex)
        {
            if ((doNotLog == false) && (this.record.AspectName == null || @override == true))
            {
                this.record.SetException(ex);
                this.record.AspectName = aspectName;
            }
            return this;
        }

        public RequestLogger SetMessage(string aspectName, bool @override, string? message)
        {
            if ((doNotLog == false) && (this.record.AspectName == null || @override == true))
            {
                this.record.Message = message;
                this.record.AspectName = aspectName;
            }
            return this;
        }

        public RequestLogger SetMessage(string aspectName, string aspectNameToOverride, string message)
        {
            if ((doNotLog == false) && (this.record.AspectName == null || this.record.AspectName == aspectNameToOverride))
            {
                this.record.Message = message;
                this.record.AspectName = aspectName;
            }
            return this;
        }

        public RequestLogger WriteLine(string line)
        {
            if (!doNotLog) this.detailsBuilder.AppendLine(line);
            return this;
        }

        public RequestLogger WithData(string key, string value)
        {
            if (!doNotLog)
            {
                this.record.Data[key] = value;
            }
            return this;
        }

        public string? GetApplicationName()
        {
            return this.applicationName;
        }
    }
}
