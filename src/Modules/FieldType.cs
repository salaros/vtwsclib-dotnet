using Newtonsoft.Json;
using System.Collections.Generic;

namespace Salaros.vTiger.WebService
{
    public class FieldType
    {
        /// <summary>
        /// Gets or sets field type name.
        /// </summary>
        /// <value>
        /// The field type name.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the symbol of the given field type.
        /// </summary>
        /// <value>
        /// The symbol of the given field type.
        /// </value>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the name of the symbol of the given field type.
        /// </summary>
        /// <value>
        /// The name of the symbol of the given field type.
        /// </value>
        [JsonProperty("symbol_name")]
        public string SymbolName { get; set; }

        /// <summary>
        /// Gets or sets the reference.
        /// </summary>
        /// <value>
        /// The reference.
        /// </value>
        [JsonProperty("refersTo")]
        public List<string> RefersTo { get; set; }

        /// <summary>
        /// Gets or sets the picklist values.
        /// </summary>
        /// <value>
        /// The picklist values.
        /// </value>
        [JsonProperty("picklistValues")]
        public IEnumerable<PicklistValue> PicklistValues { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        [JsonProperty("defaultValue")]
        public string DefaultValue { get; set; }
    }
}
