using System;
using System.Collections.Generic;
using System.Linq;
using Pantry.Queries;

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

        /// <summary>
        /// Gets a <see cref="IContinuationEnumerable{T}"/> from <paramref name="values"/>,
        /// limited by the parameters set in <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="values">The full list of values.</param>
        /// <param name="query">The query to determine skip/take in the enumerable.</param>
        /// <returns>The <see cref="IContinuationEnumerable{T}"/>.</returns>
        public static IContinuationEnumerable<TEntity> ToContinuationEnumerable<TEntity>(this IEnumerable<TEntity> values, IQuery<TEntity> query)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var pagination = LimitOffsetContinuationToken.FromContinuationQuery(query);

            var paginatedResults = values
                .Select(x => new
                {
                    Item = x,
                    TotalCount = values.Count(),
                })
                .Skip(pagination.Offset)
                .Take(pagination.Limit)
                .ToArray();

            var totalCount = paginatedResults.FirstOrDefault()?.TotalCount ?? 0;
            var items = paginatedResults.Select(x => x.Item).ToList();

            return new ContinuationEnumerable<TEntity>(
                items,
                (items.Count + pagination.Offset) < totalCount ? pagination.GetNextPageContinuationToken() : null);
        }
    }
}
