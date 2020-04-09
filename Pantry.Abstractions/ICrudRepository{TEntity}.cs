using Pantry.Traits;

namespace Pantry
{
    /// <summary>
    /// Create Read Update Delete Repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface ICrudRepository<TEntity> : ICanAdd<TEntity>, ICanGet<TEntity>, ICanUpdate<TEntity>, ICanDelete<TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
