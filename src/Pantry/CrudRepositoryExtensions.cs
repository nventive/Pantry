using System;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Exceptions;

namespace Pantry
{
    /// <summary>
    /// <see cref="ICrudRepository{TEntity}"/> extension methods.
    /// </summary>
    public static class CrudRepositoryExtensions
    {
        /// <summary>
        /// Allows the application of an Update based on the guaranteed lastest version of the entity,
        /// if the provider supports concurrency control (most do).
        /// Will retry up to <paramref name="numberOfRetries"/> on concurrency issues.
        /// </summary>
        /// <typeparam name="TEntity">The entity type. Must be <see cref="IETaggable"/>.</typeparam>
        /// <param name="repository">The <see cref="ICrudRepository{TEntity}"/>.</param>
        /// <param name="id">The entity id.</param>
        /// <param name="update">
        /// The update functions that modifies the entity.
        /// Receives the lastes version and must return the modified version.
        /// Return null to cancel the update.
        /// May be called multiple time in a tight loop.
        /// </param>
        /// <param name="numberOfRetries">The maximum number of times to retry the Get/Update cycle before giving up.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The updated entity, or null if it was cancelled inside the <paramref name="update"/> function.</returns>
        /// <exception cref="NotFoundException">If the entity could not be found.</exception>
        /// <exception cref="ConcurrencyException">If the <paramref name="numberOfRetries"/> was reached.</exception>
        public static async Task<TEntity?> UpdateLatestAsync<TEntity>(
            this ICrudRepository<TEntity> repository,
            string id,
            Func<TEntity, TEntity?> update,
            int numberOfRetries = 10,
            CancellationToken cancellationToken = default)
            where TEntity : class, IIdentifiable, IETaggable
        {
            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            while (true)
            {
                try
                {
                    var entity = await repository.GetByIdAsync(id, cancellationToken: cancellationToken).ConfigureAwait(false);
                    var updatedEntity = update(entity);
                    if (updatedEntity is null)
                    {
                        return null;
                    }

                    return await repository.UpdateAsync(updatedEntity, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                catch (ConcurrencyException)
                {
                    if (numberOfRetries > 0)
                    {
                        --numberOfRetries;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
