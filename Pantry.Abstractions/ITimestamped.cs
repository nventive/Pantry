using System;

namespace Pantry
{
    /// <summary>
    /// Has a timestamp property.
    /// </summary>
    public interface ITimestamped
    {
        /// <summary>
        /// Gets or sets the last tyime the data was updated in UTC.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }
    }
}
