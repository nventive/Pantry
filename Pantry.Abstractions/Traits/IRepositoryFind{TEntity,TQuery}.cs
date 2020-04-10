using Pantry.Queries;

namespace Pantry.Traits
{
    /// <summary>
    /// Find Repository Methods.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TQuery">The query type.</typeparam>
    public interface IRepositoryFind<TEntity, TQuery> : IRepositoryFind<TEntity, TEntity, TQuery>
        where TQuery : IQuery<TEntity>
    {
    }
}
