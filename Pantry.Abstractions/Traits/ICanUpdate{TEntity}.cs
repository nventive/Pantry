using System.Threading;
using System.Threading.Tasks;
using Pantry.Exceptions;

namespace Pantry.Traits
{
    /// <summary>
    /// Update Repository Methods.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface ICanUpdate<TEntity>
        where TEntity : IIdentifiable
    {
        /// <summary>
        /// Updates the <paramref name="entity"/> in the repository.
        /// </summary>
        /// <param name="entity">The entity to updated.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The updated entity.</returns>
        /// <exception cref="NotFoundException">If the entity was not found in the repository.</exception>
        /// <exception cref="ConcurrencyException">If the updates result in a concurrency issue.</exception>
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}
