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
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Find <typeparamref name="TResult"/> elements using the <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The query to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The results.</returns>
        Task<IContinuationEnumerable<TResult>> FindAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}
