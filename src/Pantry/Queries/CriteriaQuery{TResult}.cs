using System.Collections.Generic;

namespace Pantry.Queries
{
    /// <summary>
    /// <see cref="ICriteriaQuery{TResult}"/> implementation.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    public class CriteriaQuery<TResult> : List<ICriterion>, ICriteriaQuery<TResult>
    {
        /// <inheritdoc/>
        public int Limit { get; set; }

        /// <inheritdoc/>
        public string? ContinuationToken { get; set; }
    }
}
