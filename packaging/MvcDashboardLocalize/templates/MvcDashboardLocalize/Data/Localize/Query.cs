﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#nullable enable

namespace MyMvcApp.Data.Localize
{
    /// <summary>
    /// A query creating localization keys and values.
    /// </summary>
    [Table(nameof(Query), Schema = "localize")]
    public class Query
    {
        /// <summary>
        /// Id of the query.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public virtual int Id { get; set; }

        /// <summary>
        /// Domain id of the query.
        /// </summary>
        [JsonIgnore]
        public virtual int DomainId { get; set; }

        /// <summary>
        /// Domain of the query.
        /// </summary>
        [ForeignKey(nameof(DomainId))]
        [JsonIgnore]
        public virtual Domain? Domain { get; set; }

        /// <summary>
        /// Name of the query.
        /// </summary>
        [Required, MaxLength(2000)]
        public virtual string Name { get; set; } = null!;

        /// <summary>
        /// Connection string to use to execute this query.
        /// </summary>
        [Required, MaxLength(2000)]
        public virtual string ConnectionName { get; set; } = null!;

        /// <summary>
        /// SQL command of the query.
        /// </summary>
        [Required]
        public virtual string Sql { get; set; } = null!;
    }
}
