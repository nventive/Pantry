using System;
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

        /// <summary>
        /// Gets a <see cref="LimitOffsetContinuationToken"/> from a <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="query">The <see cref="IQuery{TResult}"/> to use.</param>
        /// <returns>A <see cref="LimitOffsetContinuationToken"/>.</returns>
        public static LimitOffsetContinuationToken FromContinuationQuery<TResult>(IQuery<TResult> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return ContinuationToken.FromContinuationToken<LimitOffsetContinuationToken>(query.ContinuationToken)
                ?? new LimitOffsetContinuationToken { Limit = query.Limit };
        }

        /// <summary>
        /// Get the next page token.
        /// </summary>
        /// <returns>The next <see cref="LimitOffsetContinuationToken"/> encoded.</returns>
        public string GetNextPageContinuationToken()
        {
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly
            return ContinuationToken.ToContinuationToken(new LimitOffsetContinuationToken { Offset = Offset + Limit, Limit = Limit })!;
        }
    }
}
