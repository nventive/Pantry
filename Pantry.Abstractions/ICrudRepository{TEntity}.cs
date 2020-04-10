using Pantry.Traits;

namespace Pantry
{
    /// <summary>
    /// Create Read Update Delete Repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface ICrudRepository<TEntity> :
        IRepositoryAdd<TEntity>,
        IRepositoryAddOrUpdate<TEntity>,
        IRepositoryGet<TEntity>,
        IRepositoryUpdate<TEntity>,
        IRepositoryRemove<TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
