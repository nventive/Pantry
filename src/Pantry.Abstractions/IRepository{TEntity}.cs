using Pantry.Traits;

namespace Pantry
{
    /// <summary>
    /// CRUD Repository that supports some level of querying.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepository<TEntity> : ICrudRepository<TEntity>, IRepositoryFindAll<TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
