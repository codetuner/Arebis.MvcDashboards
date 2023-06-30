using MyMvcApp.Data.Localize;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Data.Tasks
{
    [Table(nameof(TaskDefinition), Schema = "tasks")]
    public class TaskDefinition
    {
        [Key]
        public virtual int Id { get; set; }

        [Required]
        public virtual string? Name { get; set; }

        [Required]
        public virtual string? ImplementationClass { get; set; }

        public virtual string? Arguments { get; set; }

        public virtual string? Description { get; set; }

        public virtual bool IsActive { get; set; }
    }
}
