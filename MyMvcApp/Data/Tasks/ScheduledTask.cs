using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Data.Tasks
{
    [Table(nameof(ScheduledTask), Schema = "tasks")]
    public class ScheduledTask
    {
        public ScheduledTask()
        {
            UtcTimeCreated = UtcTimeToExecute = DateTime.UtcNow;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        public virtual int DefinitionId { get; set; }

        [ForeignKey(nameof(DefinitionId))]
        public virtual ScheduledTaskDefinition Definition { get; set; } = null!;

        [StringLength(400)]
        public virtual string? Name { get; set; }

        [StringLength(400)]
        public virtual string? QueueName { get; set; }

        [StringLength(400)]
        public virtual string? MachineNameToRunOn { get; set; }

        [StringLength(400)]
        public virtual string? MachineNameRanOn { get; set; }

        public virtual string? Arguments { get; set; }

        public virtual DateTime UtcTimeCreated { get; set; }
        
        public virtual DateTime UtcTimeToExecute { get; set; }
        
        public virtual DateTime? UtcTimeStarted { get; set; }
        
        public virtual DateTime? UtcTimeDone { get; set; }

        [NotMapped]
        public bool IsRunning => (this.UtcTimeStarted.HasValue && !this.UtcTimeDone.HasValue);

        [NotMapped]
        public TimeSpan? Duration => (this.UtcTimeStarted.HasValue ? (this.UtcTimeDone.HasValue) ? (this.UtcTimeDone.Value - this.UtcTimeStarted.Value) : (DateTime.UtcNow - this.UtcTimeStarted.Value) : null);

        public virtual bool? Succeeded { get; set; }

        public virtual string? Output { get; set; }

        public void OutputWriteLine(string str)
        {
            Output = Output ?? String.Empty;
            Output += str + Environment.NewLine;
        }

        /// <summary>
        /// Rowversion for concurrency check.
        /// </summary>
        [Timestamp]
        public virtual byte[]? Version { get; set; }
    }
}
