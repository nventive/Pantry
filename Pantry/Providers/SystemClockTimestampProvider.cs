using System;

namespace Pantry.Providers
{
    /// <summary>
    /// <see cref="ITimestampProvider"/> implementation that uses the standard system clock.
    /// </summary>
    public class SystemClockTimestampProvider : ITimestampProvider
    {
        /// <inheritdoc/>
        public DateTimeOffset CurrentTimestamp() => DateTimeOffset.UtcNow;
    }
}
