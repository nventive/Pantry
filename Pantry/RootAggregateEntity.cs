using System;

namespace Pantry
{
    /// <summary>
    /// Base class for root aggregate entities that are <see cref="IIdentifiable"/> and <see cref="IETaggable"/>.
    /// </summary>
    public abstract class RootAggregateEntity : IIdentifiable, IETaggable, ITimestamped
    {
        /// <inheritdoc/>
        public string Id { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string? ETag { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset? Timestamp { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"[{GetType().Name}] ({(string.IsNullOrEmpty(Id) ? "<new>" : Id)})";
    }
}
