using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Continuation;
using Pantry.Queries;

namespace Pantry.Traits
{
    /// <summary>
    /// FindAll Repository Methods.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepositoryFindAll<TEntity>
    {
        /// <summary>
        /// Find all <typeparamref name="TEntity"/> elements, by chunks.
        /// </summary>
        /// <param name="continuationToken">The continuation token.</param>
        /// <param name="limit">The number of elements per chunk.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The results.</returns>
        Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = RepositoryQuery.DefaultLimit, CancellationToken cancellationToken = default);
    }
}
