using System.Threading;
using System.Threading.Tasks;
using Pantry.Exceptions;

namespace Pantry.Traits
{
    /// <summary>
    /// Add Repository Methods.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface ICanAdd<T>
        where T : IIdentifiable
    {
        /// <summary>
        /// Adds the <paramref name="entity"/> to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The added entity.</returns>
        /// <exception cref="ConflictException">If the entity already exists.</exception>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    }
}
