using System.Collections.Generic;

namespace Pantry.Continuation
{
    /// <summary>
    /// <see cref="IEnumerable{T}"/> extension methods.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Creates a <see cref="IContinuationEnumerable{T}"/> with a <paramref name="continuationToken"/>.
        /// </summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <param name="items">The original <see cref="IEnumerable{T}"/>.</param>
        /// <param name="continuationToken">The continuation token, if any.</param>
        /// <returns>The <see cref="IContinuationEnumerable{T}"/>.</returns>
        public static IContinuationEnumerable<T> ToContinuationEnumerable<T>(this IEnumerable<T> items, string? continuationToken = null)
        {
            return new ContinuationEnumerable<T>(items, continuationToken);
        }
    }
}
