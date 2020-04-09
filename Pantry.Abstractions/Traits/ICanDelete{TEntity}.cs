using System;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Exceptions;

namespace Pantry.Traits
{
    /// <summary>
    /// Delete Repository Methods.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface ICanDelete<TEntity>
        where TEntity : IIdentifiable
    {
        /// <summary>
        /// Deletes the entity by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="NotFoundException">If the entity is not found.</exception>
        async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!await TryDeleteAsync(id, cancellationToken).ConfigureAwait(false))
            {
                throw new NotFoundException(targetType: typeof(TEntity).Name, targetId: id);
            }
        }

        /// <summary>
        /// Deletes the <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="NotFoundException">If the entity is not found.</exception>
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return DeleteAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        /// Deletes the <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>True if the entity was deleted, false if it was not found.</returns>
        Task<bool> TryDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return TryDeleteAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        /// Deletes the entity by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>True if the entity was deleted, false if it was not found.</returns>
        Task<bool> TryDeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
