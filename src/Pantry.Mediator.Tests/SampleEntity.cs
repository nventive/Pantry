using System;

namespace Pantry.Mediator.Tests
{
    public class SampleEntity : IIdentifiable, IETaggable, ITimestamped
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
