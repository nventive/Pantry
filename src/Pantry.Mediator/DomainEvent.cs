using System;

namespace Pantry.Mediator
{
    /// <summary>
    /// Base class for <see cref="IDomainEvent"/> implementations.
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        /// <inheritdoc/>
        public DateTimeOffset? Timestamp { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] ({Timestamp})";
    }
}
