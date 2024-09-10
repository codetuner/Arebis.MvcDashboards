using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Data.Tasks
{
    [Table(nameof(ScheduledTaskDefinition), Schema = "tasks")]
    public class ScheduledTaskDefinition
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        [Required, StringLength(400)]
        public virtual string? Name { get; set; }

        [Required, StringLength(2000)]
        public virtual string? ImplementationClass { get; set; }

        [StringLength(400)]
        public virtual string? ProcessRole { get; set; }

        public virtual string? Arguments { get; set; }

        public virtual string? Description { get; set; }

        public virtual bool IsActive { get; set; }
    }
}
