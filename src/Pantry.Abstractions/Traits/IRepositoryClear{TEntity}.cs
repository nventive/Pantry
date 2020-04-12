using System.Threading;
using System.Threading.Tasks;

namespace Pantry.Traits
{
    /// <summary>
    /// Clear Repository Methods.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepositoryClear<TEntity>
    {
        /// <summary>
        /// Clears all entities from the repository.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
