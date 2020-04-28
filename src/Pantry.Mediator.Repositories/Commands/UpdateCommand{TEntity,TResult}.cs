namespace Pantry.Mediator.Repositories.Commands
{
    /// <summary>
    /// Standard command to update an entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public abstract class UpdateCommand<TEntity, TResult> : IDomainCommand<TResult>, IDomainRepositoryRequest, IIdentifiable
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Gets or sets the id of the entity to update.
        /// </summary>
        public string Id { get; set; } = string.Empty;
    }
}
