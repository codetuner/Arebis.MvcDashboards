﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#nullable enable

namespace MyMvcApp.Data.Localize
{
    /// <summary>
    /// Represents a localized value for a localization key.
    /// </summary>
    [Table(nameof(KeyValue), Schema = "localize")]
    public class KeyValue
    {
        /// <summary>
        /// Id of the value.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public virtual int Id { get; set; }

        /// <summary>
        /// Key id of the value.
        /// </summary>
        [JsonIgnore]
        public virtual int KeyId { get; set; }

        /// <summary>
        /// Key of the value.
        /// </summary>
        [ForeignKey(nameof(KeyId))]
        [JsonIgnore]
        public virtual Key? Key { get; set; }

        /// <summary>
        /// Culture of this value.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual string Culture { get; set; } = null!;

        /// <summary>
        /// Localized value string.
        /// </summary>
        public virtual string? Value { get; set; }

        /// <summary>
        /// Whether this value is reviewed.
        /// </summary>
        public virtual bool Reviewed { get; set; }
    }
}
