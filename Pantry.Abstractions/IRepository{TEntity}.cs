using Pantry.Traits;

namespace Pantry
{
    /// <summary>
    /// CRUD Repository that supports queries.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepository<TEntity> : ICrudRepository<TEntity>, IRepositoryFind<TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
