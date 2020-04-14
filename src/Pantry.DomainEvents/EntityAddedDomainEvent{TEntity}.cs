namespace Pantry.DomainEvents
{
    /// <summary>
    /// An entity has been added to a repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class EntityAddedDomainEvent<TEntity> : DomainEvent
        where TEntity : class
    {
        /// <summary>
        /// Gets or sets the added entity.
        /// </summary>
        public TEntity? Entity { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {Entity}";
    }
}
