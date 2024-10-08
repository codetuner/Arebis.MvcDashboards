﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#nullable enable

namespace MyMvcApp.Data.Localize
{
    /// <summary>
    /// Represents a localization domain regrouping keys and queries to use together.
    /// </summary>
    [Table(nameof(Domain), Schema = "localize")]
    public class Domain
    {
        /// <summary>
        /// Id of the domain.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public virtual int Id { get; set; }

        /// <summary>
        /// Name of the domain.
        /// </summary>
        [Required, MaxLength(200)]
        public virtual string Name { get; set; } = null!;

        /// <summary>
        /// List of localization queries.
        /// </summary>
        [InverseProperty(nameof(Query.Domain))]
        public virtual IList<Query>? Queries { get; set; }

        /// <summary>
        /// List of localization keys.
        /// </summary>
        [InverseProperty(nameof(Key.Domain))]
        public virtual IList<Key>? Keys { get; set; }

        /// <summary>
        /// List of exposed cultures.
        /// </summary>
        [MaxLength(2000)]
        public virtual string[]? Cultures { get; set; }
    }
}
