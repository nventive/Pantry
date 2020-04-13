using System;

namespace Pantry.DomainEvents
{
    /// <summary>
    /// Base class for <see cref="IDomainEvent"/> implementations.
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        /// <inheritdoc/>
        public string Id { get; set; } = string.Empty;

        /// <inheritdoc/>
        public DateTimeOffset? Timestamp { get; set; }
    }
}
