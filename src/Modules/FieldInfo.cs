using Newtonsoft.Json;
using System;

namespace Salaros.vTiger.WebService
{
    public class FieldInfo
    {
        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the label of the field.
        /// </summary>
        /// <value>
        /// The label of the field.
        /// </value>
        [JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is mandatory.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is mandatory; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("mandatory")]
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Gets or sets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        [JsonProperty("type")]
        public FieldType FieldType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is nullable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is nullable; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("nullable")]
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is editable; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("editable")]
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets the field identifier.
        /// </summary>
        /// <value>
        /// The field identifier.
        /// </value>
        [JsonProperty("fieldid")]
        public string FieldId { get; set; }

        /// <summary>
        /// Gets or sets the type of the UI.
        /// </summary>
        /// <value>
        /// The type of the UI.
        /// </value>
        [JsonProperty("uitype")]
        public int UiTypeRaw { get; set; }

        /// <summary>
        /// Gets or sets the type of the UI.
        /// </summary>
        /// <value>
        /// The type of the UI.
        /// </value>
        public UiType UiType => Enum.IsDefined(typeof(UiType), UiTypeRaw) ? (UiType)UiTypeRaw : UiType.Unknown;

        /// <summary>
        /// Gets or sets the block identifier (id of the parent block).
        /// </summary>
        /// <value>
        /// The block identifier.
        /// </value>
        [JsonProperty("blockid")]
        public string BlockId { get; set; }

        /// <summary>
        /// Gets or sets the index of the field on the form.
        /// It is used to sort / order fields.
        /// </summary>
        /// <value>
        /// Sorting index sequence of the given field.
        /// </value>
        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        [JsonProperty("default")]
        public string DefaultValue { get; set; }
    }
}
