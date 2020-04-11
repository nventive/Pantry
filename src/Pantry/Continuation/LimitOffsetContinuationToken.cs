using Pantry.Queries;

namespace Pantry.Continuation
{
    /// <summary>
    /// Continuation token support adapted for offset/limit paginations systems.
    /// </summary>
    public class LimitOffsetContinuationToken
    {
        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        public int Limit { get; set; } = Query.DefaultLimit;

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        public int Offset { get; set; } = 0;
    }
}
