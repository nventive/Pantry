using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        /// <param name="encoder">The continuation token encoder.</param>
        /// <returns>The <see cref="IContinuationEnumerable{T}"/>.</returns>
        public static async ValueTask<IContinuationEnumerable<TEntity>> ToContinuationEnumerable<TEntity>(
            this IEnumerable<TEntity> values,
            IRepositoryQuery<TEntity> query,
            IContinuationTokenEncoder<LimitOffsetContinuationToken> encoder)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            var pagination = (await encoder.Decode(query.ContinuationToken)) ?? new LimitOffsetContinuationToken { Limit = query.Limit };

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
                (items.Count + pagination.Offset) < totalCount ? await GetNextPageContinuationToken(pagination, encoder) : null);
        }

        private static ValueTask<string?> GetNextPageContinuationToken(
            LimitOffsetContinuationToken token,
            IContinuationTokenEncoder<LimitOffsetContinuationToken> encoder)
        {
            return encoder.Encode(new LimitOffsetContinuationToken { Offset = token.Offset + token.Limit, Limit = token.Limit });
        }
    }
}
