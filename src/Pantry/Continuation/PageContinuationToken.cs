using Pantry.Queries;

namespace Pantry.Continuation
{
    /// <summary>
    /// Continuation token support adapted for page number/limit paginations systems.
    /// </summary>
    public class PageContinuationToken
    {
        /// <summary>
        /// Gets or sets the number of item per page.
        /// </summary>
        public int PerPage { get; set; } = Query.DefaultLimit;

        /// <summary>
        /// Gets or sets the page number. Starts at 1.
        /// </summary>
        public int Page { get; set; } = 1;
    }
}
