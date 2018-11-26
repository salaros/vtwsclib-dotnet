using Newtonsoft.Json.Linq;
using System.Linq;

namespace Salaros.Vtiger.VTWSCLib
{
    public class CrmEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrmEntity"/> class.
        /// </summary>
        /// <param name="json">The JSON.</param>
        internal CrmEntity(JToken json)
        {
            Json = json;
        }

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified property.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="prop">The property.</param>
        /// <returns></returns>
        public string this[string prop] => Json?.Value<string>(prop);

        /// <summary>
        /// Gets the JSON.
        /// </summary>
        /// <value>
        /// The JSON.
        /// </value>
        public JToken Json { get; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public long Id => NumericId;

        /// <summary>
        /// Gets the numeric identifier.
        /// </summary>
        /// <value>
        /// The numeric identifier.
        /// </value>
        public long NumericId => long.TryParse(TypedId?.Split('x')?.LastOrDefault() ?? string.Empty, out long id) ? id : -1;

        /// <summary>
        /// Gets the typed identifier.
        /// </summary>
        /// <value>
        /// The typed identifier.
        /// </value>
        public string TypedId => Json?.Value<string>("id");

        /// <summary>
        /// Gets the entity owner's identifier.
        /// </summary>
        /// <value>
        /// The entity owner's identifier.
        /// </value>
        public string OwnerId => Json?.Value<string>("assigned_user_id");

        /// <summary>
        /// Gets the time when the entity was last modified.
        /// </summary>
        /// <value>
        /// The time when the entity was last modified.
        /// </value>
        public string ModifiedTime => Json?.Value<string>("modifiedtime");

        /// <summary>
        /// Gets the time when the entity was created.
        /// </summary>
        /// <value>
        /// The time when the entity was created.
        /// </value>
        public string CreatedTime => Json?.Value<string>("createdtime");
    }
}
