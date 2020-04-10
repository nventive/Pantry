namespace Pantry.Queries
{
    /// <summary>
    /// Base class for <see cref="IQuery{TEntity}"/> implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public abstract class Query<TEntity> : Query<TEntity, TEntity>, IQuery<TEntity>
    {
    }
}
