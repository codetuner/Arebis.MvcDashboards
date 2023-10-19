using MyMvcApp.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Data.Logging
{
    /// <summary>
    /// Defines action when certain log events occur.
    /// </summary>
    [Table(nameof(LogActionRule), Schema = "logging")]
    public class LogActionRule
    {
        /// <summary>
        /// Id of the log.
        /// </summary>
        [Key]
        public virtual int Id { get; set; }

        /// <summary>
        /// Name of the log aspect this rule checks for.
        /// </summary>
        public virtual string? AspectName { get; set; }

        /// <summary>
        /// [NotMapped] Log aspect this rule checks for.
        /// </summary>
        [NotMapped]
        public LogAspect? Aspect
        {
            get
            {
                return (this.AspectName != null) ? LogAspect.ByName(this.AspectName) : null;
            }
            set
            {
                this.AspectName = value?.Name;
            }
        }

        /// <summary>
        /// Host on which the log event was created this rule checks for.
        /// </summary>
        [MaxLength(2000)]
        public virtual string? Host { get; set; }

        /// <summary>
        /// Type information of the log. I.e. exception type this rule checks for.
        /// </summary>
        [MaxLength(2000)]
        public virtual string? Type { get; set; }

        /// <summary>
        /// HTTP request method this rule checks for.
        /// </summary>
        [MaxLength(20)]
        public virtual string? Method { get; set; }

        /// <summary>
        /// URL of the logged request this rule checks for.
        /// </summary>
        [Required, MaxLength(2000)]
        public virtual string? Url { get; set; }

        /// <summary>
        /// HTTP status code of the request response this rule checks for.
        /// </summary>
        public virtual int? StatusCode { get; set; }

        /// <summary>
        /// </summary>
        public virtual LogAction Action { get; set; } = LogAction.NoAction;

        /// <summary>
        /// Optional data used to perform the action.
        /// I.e. fora "SendEmailAlarm" action, this field could contain the email address to send to.
        /// </summary>
        public virtual string? ActionData { get; set; }

        /// <summary>
        /// Whether the rule is active.
        /// </summary>
        public virtual bool IsActive { get; set; }

        /// <summary>
        /// Whether the rule matches the given request/response objects.
        /// </summary>
        public bool Matches(HttpRequest request, HttpResponse response, string? aspectName, string? type)
        {
            if (!this.IsActive) return false;
            if (this.Url != null && this.Url != "*" && !request.Path.ToString().Contains(this.Url, StringComparison.OrdinalIgnoreCase)) return false;
            if (this.Method is not null && !this.Method.Equals(request.Method, StringComparison.OrdinalIgnoreCase)) return false;
            if (this.AspectName is not null && !this.AspectName.Equals(aspectName, StringComparison.OrdinalIgnoreCase)) return false;
            if (this.StatusCode.HasValue && this.StatusCode != response.StatusCode) return false;
            if (this.Host is not null && !this.Host.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase)) return false;
            if (this.Type is not null && !this.Type.Equals(type, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }
    }
}
