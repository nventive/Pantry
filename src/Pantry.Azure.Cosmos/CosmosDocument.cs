using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pantry.Azure.Cosmos
{
    /// <summary>
    /// The actual document stored in CosmosDB.
    /// </summary>
    public class CosmosDocument : IIdentifiable
    {
        /// <summary>
        /// Gets the attribute name for the id. <see cref="Id"/>.
        /// </summary>
        public const string IdAttribute = "id";

        /// <summary>
        /// Gets the attribute name for the entity type. <see cref="EntityType"/>.
        /// </summary>
        public const string TypeAttribute = "_type";

        /// <summary>
        /// Gets the attribute name for the CosmosDB system ETag.
        /// </summary>
        public const string SystemETagAttribute = "_etag";

        /// <summary>
        /// Gets the attribute name for the CosmosDb system Timestamp.
        /// </summary>
        public const string SystemTimestampAttribute = "_ts";

        /// <summary>
        /// Gets or sets the entity type that the document refer to.
        /// </summary>
        [JsonProperty(PropertyName = TypeAttribute)]
        public string EntityType { get; set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty(PropertyName = IdAttribute)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the document properties.
        /// </summary>
        [JsonExtensionData]
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Conflict with serialization.")]
        public Dictionary<string, JToken> Attributes { get; set; } = new Dictionary<string, JToken>();

        /// <inheritdoc />
        public override string ToString() => $"[{GetType().Name}] Of {EntityType} ({(string.IsNullOrEmpty(Id) ? "<new>" : Id)})";
    }
}
