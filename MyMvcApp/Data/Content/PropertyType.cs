using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace MyMvcApp.Data.Content
{
    /// <summary>
    /// Type of a property.
    /// </summary>
    [Table(nameof(PropertyType), Schema = "content")]
    public class PropertyType
    {
        /// <summary>
        /// Identifier of the property type.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        /// <summary>
        /// Name of the property type.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual required string Name { get; set; }

        /// <summary>
        /// Order in which the property type and its properties are displayed.
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Document type this property type belongs to.
        /// </summary>
        public virtual int DocumentTypeId { get; set; }

        /// <summary>
        /// Document type this property type belongs to.
        /// </summary>
        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType? DocumentType { get; set; }

        /// <summary>
        /// Data type of the property value.
        /// </summary>
        public virtual int DataTypeId { get; set; }

        /// <summary>
        /// Data type of the property value.
        /// </summary>
        [ForeignKey(nameof(DataTypeId))]
        public virtual DataType? DataType { get; set; }

        /// <summary>
        /// Json settings for this data type.
        /// The settings are passed to the ViewData of the EditorTemplates and DisplayTemplates.
        /// </summary>
        public virtual Dictionary<string, string> Settings { get; set; } = [];

        /// <summary>
        /// Settings of this property type combined with the settings of its datatype.
        /// </summary>
        [NotMapped]
        public Dictionary<string, object> CombinedSettings
        {
            get
            {
                var settings = new Dictionary<string, object>();
                if (this.DataType != null)
                {
                    foreach (var pair in this.DataType.Settings)
                    {
                        settings[pair.Key] = pair.Value;
                    }
                }
                foreach (var pair in this.Settings)
                {
                    settings[pair.Key] = pair.Value;
                }
                return settings;
            }
        }
    }
}
