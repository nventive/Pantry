namespace Pantry.DomainEvents
{
    /// <summary>
    /// An entity has been updated in a repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class EntityUpdatedDomainEvent<TEntity> : DomainEvent
        where TEntity : class
    {
        /// <summary>
        /// Gets or sets the updated entity.
        /// </summary>
        public TEntity? Entity { get; set; }
    }
}
