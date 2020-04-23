using System;

namespace Pantry.AspNetCore.Models
{
    /// <summary>
    /// Model for <see cref="ResourceCollection{TAttributes}"/> itemss.
    /// </summary>
    /// <typeparam name="TAttributes">The attributes type.</typeparam>
    public class ResourceCollectionItem<TAttributes> : IIdentifiable, IETaggable, ITimestamped
        where TAttributes : class
    {
        /// <inheritdoc/>
        public string Id { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string? ETag { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the resource type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        public TAttributes? Attributes { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"[{GetType().Name}] ({(string.IsNullOrEmpty(Id) ? "<new>" : Id)})";
    }
}
