namespace Pantry.Mediator.Repositories.Queries
{
    /// <summary>
    /// Standard query to get an entity by id.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public abstract class GetByIdDomainQuery<TEntity> : GetByIdDomainQuery<TEntity, TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
