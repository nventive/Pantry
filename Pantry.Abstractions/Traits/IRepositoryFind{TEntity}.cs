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
    public interface IRepositoryFind<TEntity>
        where TEntity : IIdentifiable
    {
        /// <summary>
        /// Find <typeparamref name="TResult"/> elements using the <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TResult">The elements return types.</typeparam>
        /// <param name="query">The query to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The results.</returns>
        Task<IContinuationEnumerable<TResult>> FindAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
    }
}
