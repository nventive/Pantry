using System;

namespace Pantry.Mediator.Repositories.DomainEvents
{
    /// <summary>
    /// An entity has been added to a repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class EntityAddedDomainEvent<TEntity> : DomainEvent
        where TEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAddedDomainEvent{TEntity}"/> class.
        /// </summary>
        /// <param name="entity">The added entity.</param>
        public EntityAddedDomainEvent(TEntity entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        /// <summary>
        /// Gets or sets the added entity.
        /// </summary>
        public TEntity Entity { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {Entity}";
    }
}
