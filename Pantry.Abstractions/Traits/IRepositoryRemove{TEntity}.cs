using System;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Exceptions;

namespace Pantry.Traits
{
    /// <summary>
    /// Remove Repository Methods.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepositoryRemove<TEntity>
        where TEntity : IIdentifiable
    {
        /// <summary>
        /// Remove the entity by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="NotFoundException">If the entity is not found.</exception>
        async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!await TryRemoveAsync(id, cancellationToken).ConfigureAwait(false))
            {
                throw new NotFoundException(targetType: typeof(TEntity).Name, targetId: id);
            }
        }

        /// <summary>
        /// Remove the <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="NotFoundException">If the entity is not found.</exception>
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return RemoveAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        /// Remove the <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>True if the entity was deleted, false if it was not found.</returns>
        Task<bool> TryRemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return TryRemoveAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        /// Remove the entity by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>True if the entity was deleted, false if it was not found.</returns>
        Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default);
    }
}
