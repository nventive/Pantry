using Pantry.Queries;

namespace Pantry.Traits
{
    /// <summary>
    /// Repository that supports default <see cref="ICriteriaRepositoryQuery{TResult}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepositoryFindByCriteria<TEntity> : IRepositoryFind<TEntity, TEntity, ICriteriaRepositoryQuery<TEntity>>
    {
    }
}
