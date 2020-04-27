using System;

namespace Pantry.Mediator.Repositories.DomainEvents
{
    /// <summary>
    /// An entity has been removed from a repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class EntityRemovedDomainEvent<TEntity> : DomainEvent
        where TEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRemovedDomainEvent{TEntity}"/> class.
        /// </summary>
        /// <param name="entityId">The entity id.</param>
        public EntityRemovedDomainEvent(string entityId)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        }

        /// <summary>
        /// Gets or sets the removed entity id.
        /// </summary>
        public string EntityId { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {EntityId}";
    }
}
