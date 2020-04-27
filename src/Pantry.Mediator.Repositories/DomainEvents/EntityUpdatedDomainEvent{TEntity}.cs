namespace Pantry.Mediator.Repositories.DomainEvents
{
    /// <summary>
    /// An entity has been updated in a repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class EntityUpdatedDomainEvent<TEntity> : DomainEvent
        where TEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityUpdatedDomainEvent{TEntity}"/> class.
        /// </summary>
        /// <param name="entity">The updated entity.</param>
        public EntityUpdatedDomainEvent(TEntity entity)
        {
            Entity = entity ?? throw new System.ArgumentNullException(nameof(entity));
        }

        /// <summary>
        /// Gets or sets the updated entity.
        /// </summary>
        public TEntity Entity { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {Entity}";
    }
}
