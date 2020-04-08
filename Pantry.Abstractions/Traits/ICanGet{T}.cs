using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Exceptions;

namespace Pantry.Traits
{
    /// <summary>
    /// Get Repository Methods.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface ICanGet<T>
        where T : class, IIdentifiable
    {
        /// <summary>
        /// Gets an entity by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The entity if it is found.</returns>
        /// <exception cref="NotFoundException">If the entity is not found.</exception>
        async Task<T> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await TryGetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (result is null)
            {
                throw new NotFoundException(targetType: typeof(T).Name, targetId: id);
            }

            return result;
        }

        /// <summary>
        /// Gets multiple entities by their <paramref name="ids"/>.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The association of id and entity if it is found, null if it is not found.</returns>
        async Task<IDictionary<string, T?>> TryGetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            var results = await Task.WhenAll(ids
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(async id => (id, entity: await TryGetByIdAsync(id, cancellationToken).ConfigureAwait(false))))
                .ConfigureAwait(false);

            return results.ToDictionary(x => x.id, x => x.entity);
        }

        /// <summary>
        /// Indicates whether <typeparamref name="T"/> exists by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>True it <typeparamref name="T"/> exists, false otherwise.</returns>
        async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            return await TryGetByIdAsync(id, cancellationToken).ConfigureAwait(false) != null;
        }

        /// <summary>
        /// Gets an entity by its <paramref name="id"/>, or null if not found.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The entity if it is found, null if it is not found.</returns>
        Task<T?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default);
    }
}
