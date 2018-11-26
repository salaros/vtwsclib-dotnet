using Newtonsoft.Json;
using System.Collections.Generic;

namespace Salaros.Vtiger.VTWSCLib
{
    public class ModuleInfo
    {
        /// <summary>
        /// Gets or sets the identifier prefix.
        /// </summary>
        /// <value>
        /// The identifier prefix.
        /// </value>
        [JsonProperty("idPrefix")]
        public string IdPrefix { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        [JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is createable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is createable; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("createable")]
        public bool IsCreateable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is updateable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is updateable; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("updateable")]
        public bool IsUpdateable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleteable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is deleteable; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("deleteable")]
        public bool IsDeleteable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is retrieveable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is retrieveable; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("retrieveable")]
        public bool IsRetrieveable { get; set; }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        [JsonProperty("fields")]
        public List<FieldInfo> Fields { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is entity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is entity; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("isEntity")]
        public bool IsEntity { get; set; }

        /// <summary>
        /// Gets or sets the label fields.
        /// </summary>
        /// <value>
        /// The label fields.
        /// </value>
        [JsonProperty("labelFields")]
        public string LabelFields { get; set; }
    }
}
