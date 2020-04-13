using System.Threading;
using System.Threading.Tasks;
using Pantry.Exceptions;

namespace Pantry.Traits
{
    /// <summary>
    /// Add or Update Repository Methods.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepositoryAddOrUpdate<TEntity>
        where TEntity : IIdentifiable
    {
        /// <summary>
        /// Adds or Updates the <paramref name="entity"/> in the repository.
        /// </summary>
        /// <param name="entity">The entity to add or update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The updated entity, along with a boolean indicating that the entity was added (true) or updated (false).</returns>
        /// <exception cref="ConcurrencyException">If the updates result in a concurrency issue.</exception>
        Task<(TEntity, bool)> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}
