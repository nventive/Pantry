using Pantry.Queries;

namespace Pantry.Traits
{
    /// <summary>
    /// Repository that supports default <see cref="ICriteriaQuery{TResult}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepositoryFindByCriteria<TEntity> : IRepositoryFind<TEntity, TEntity, ICriteriaQuery<TEntity>>
    {
    }
}
