﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Data.Tasks
{
    [Table(nameof(Task), Schema = "tasks")]
    public class Task
    {
        public Task()
        {
            UtcTimeCreated = UtcTimeToExecute = DateTime.UtcNow;
        }

        [Key]
        public virtual int Id { get; set; }

        public virtual int DefinitionId { get; set; }

        [ForeignKey(nameof(DefinitionId))]
        public virtual TaskDefinition Definition { get; set; } = null!;

        public virtual string? Name { get; set; }

        public virtual string? QueueName { get; set; }
        
        public virtual string? MachineName { get; set; }

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
        public byte[]? Version { get; set; }
    }
}
