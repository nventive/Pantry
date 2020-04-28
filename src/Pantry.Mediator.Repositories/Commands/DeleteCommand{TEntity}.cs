namespace Pantry.Mediator.Repositories.Commands
{
    /// <summary>
    /// Standard command to delete an entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public abstract class DeleteCommand<TEntity> : IDomainCommand<bool>, IDomainRepositoryRequest, IIdentifiable
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Gets or sets the id of the entity to delete.
        /// </summary>
        public string Id { get; set; } = string.Empty;
    }
}
