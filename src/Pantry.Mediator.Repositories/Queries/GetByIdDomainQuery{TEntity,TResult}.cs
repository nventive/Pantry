namespace Pantry.Mediator.Repositories.Queries
{
    /// <summary>
    /// Standard query to get an entity by id.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public abstract class GetByIdDomainQuery<TEntity, TResult> : IDomainQuery<TResult>, IRepositoryRequest, IIdentifiable
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Gets or sets the id of the entity to get.
        /// </summary>
        public string Id { get; set; } = string.Empty;
    }
}
