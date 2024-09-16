using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Data.Content
{
    /// <summary>
    /// Represents the type of a property value.
    /// </summary>
    [Table(nameof(DataType), Schema = "content")]
    public class DataType
    {
        /// <summary>
        /// Identifier of the data type.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        /// <summary>
        /// Name of the data type.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual required string Name { get; set; }

        /// <summary>
        /// Name of the EditorTemplate to use for editing content, and DisplayTemplate to use for rendering.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual required string Template { get; set; }

        /// <summary>
        /// Settings for this data type.
        /// The settings are passed to the ViewData of the EditorTemplates and DisplayTemplates.
        /// </summary>
        public virtual Dictionary<string, string> Settings { get; set; } = [];
    }
}
