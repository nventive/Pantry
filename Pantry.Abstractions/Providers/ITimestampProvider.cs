using System;

namespace Pantry.Providers
{
    /// <summary>
    /// Provides timestamps.
    /// </summary>
    public interface ITimestampProvider
    {
        /// <summary>
        /// Gets the current timestamp in UTC.
        /// </summary>
        /// <returns>The current <see cref="DateTimeOffset"/> in UTC.</returns>
        DateTimeOffset CurrentTimestamp();
    }
}
