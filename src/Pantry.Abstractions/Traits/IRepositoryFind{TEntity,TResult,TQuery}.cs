using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Continuation;
using Pantry.Queries;

namespace Pantry.Traits
{
    /// <summary>
    /// Find Repository Methods.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResult">The query return type.</typeparam>
    /// <typeparam name="TQuery">The query type.</typeparam>
    public interface IRepositoryFind<TEntity, TResult, TQuery>
        where TQuery : IRepositoryQuery<TResult>
    {
        /// <summary>
        /// Find all <typeparamref name="TResult"/> elements using the <paramref name="query"/>,
        /// until there is no more continuation token, starting from the current query continuation token.
        /// Retrieval is paginated, and can take a long time. You probably should think twice before using it.
        /// You can use the <see cref="IRepositoryQuery{TResult}.Limit"/> parameter to control the batch size.
        /// </summary>
        /// <param name="query">The query to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The results.</returns>
        async IAsyncEnumerable<TResult> FindRemainingAsync(TQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            do
            {
                var pageResult = await FindAsync(query, cancellationToken).ConfigureAwait(false);
                foreach (var item in pageResult)
                {
                    yield return item;
                }

                query.ContinuationToken = pageResult.ContinuationToken;
            }
            while (!string.IsNullOrEmpty(query.ContinuationToken));
        }

        /// <summary>
        /// Find <typeparamref name="TResult"/> elements using the <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The query to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The results.</returns>
        Task<IContinuationEnumerable<TResult>> FindAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}
