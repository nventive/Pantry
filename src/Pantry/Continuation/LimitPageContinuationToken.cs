using Pantry.Queries;

namespace Pantry.Continuation
{
    /// <summary>
    /// Continuation token support adapted for page number/limit paginations systems.
    /// </summary>
    public class LimitPageContinuationToken
    {
        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        public int Limit { get; set; } = Query.DefaultLimit;

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        public int Page { get; set; } = 0;
    }
}
