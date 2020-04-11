using System.Threading;
using System.Threading.Tasks;
using Pantry.Exceptions;

namespace Pantry.Traits
{
    /// <summary>
    /// Add Repository Methods.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepositoryAdd<TEntity>
        where TEntity : IIdentifiable
    {
        /// <summary>
        /// Adds the <paramref name="entity"/> to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The added entity.</returns>
        /// <exception cref="ConflictException">If the entity already exists.</exception>
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}
