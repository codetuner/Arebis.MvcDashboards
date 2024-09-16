using System;

namespace MyMvcApp.Tasks
{
    /// <summary>
    /// Argument of a task.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TaskArgumentAttribute : Attribute
    {
        /// <summary>
        /// Name of the argument. If null, same as the decorated property name.
        /// </summary>
        public string? Name { get; set; }
    }
}
