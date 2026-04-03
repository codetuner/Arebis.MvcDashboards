using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyMvcApp.Data.Localize
{
    /// <summary>
    /// Represents a background job for processing localization data.
    /// </summary>
    [Table(nameof(BackgroundJob), Schema = "localize")]
    public class BackgroundJob
    {
        /// <summary>
        /// Id of the job.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        /// <summary>
        /// Display name of the job.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual string? Name { get; set; }

        /// <summary>
        /// Type of the job, used to determine which processing logic to apply.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual string? JobType { get; set; }

        /// <summary>
        /// Extra parameters for the job execution, as a JSON formatted string dictionary.
        /// </summary>
        public virtual string? Parameters { get; set; }

        /// <summary>
        /// The UTC time when the job started.
        /// </summary>
        public virtual DateTime UtcTimeStarted { get; set; }

        /// <summary>
        /// The UTC time when the job ended.
        /// </summary>
        public virtual DateTime? UtcTimeEnded { get; set; }

        /// <summary>
        /// Outcome of the job execution.
        /// </summary>
        public virtual bool? Succeeded { get; set; }

        /// <summary>
        /// Output of the job execution, such as logs or error messages.
        /// </summary>
        public virtual string? Output { get; set; }
    }
}
