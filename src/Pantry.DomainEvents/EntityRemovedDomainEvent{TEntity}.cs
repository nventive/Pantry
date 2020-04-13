namespace Pantry.DomainEvents
{
    /// <summary>
    /// An entity has been removed from a repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class EntityRemovedDomainEvent<TEntity> : DomainEvent
        where TEntity : class
    {
        /// <summary>
        /// Gets or sets the removed entity id.
        /// </summary>
        public string? EntityId { get; set; }
    }
}
