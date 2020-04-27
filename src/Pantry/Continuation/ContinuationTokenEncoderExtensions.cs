using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pantry.Queries;

namespace Pantry.Continuation
{
    /// <summary>
    /// <see cref="IContinuationTokenEncoder{TContinuationToken}"/> extension methods.
    /// </summary>
    public static class ContinuationTokenEncoderExtensions
    {
        /// <summary>
        /// Gets a <see cref="IContinuationEnumerable{T}"/> from <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="encoder">The continuation token encoder.</param>
        /// <param name="values">The full list of values.</param>
        /// <param name="continuationToken">The continuation token previously produced by the SAME encoder.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The <see cref="IContinuationEnumerable{T}"/>.</returns>
        public static async ValueTask<IContinuationEnumerable<TEntity>> ToContinuationEnumerable<TEntity>(
            this IContinuationTokenEncoder<LimitOffsetContinuationToken> encoder,
            IEnumerable<TEntity> values,
            string? continuationToken,
            int limit = RepositoryQuery.DefaultLimit)
        {
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var pagination = (await encoder.Decode(continuationToken)) ?? new LimitOffsetContinuationToken { Limit = limit };

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

            string? nextEncodedToken = null;
            if ((items.Count + pagination.Offset) < totalCount)
            {
                nextEncodedToken = await encoder.Encode(
                    new LimitOffsetContinuationToken { Offset = pagination.Offset + pagination.Limit, Limit = pagination.Limit });
            }

            return new ContinuationEnumerable<TEntity>(items, nextEncodedToken);
        }
    }
}
