namespace Pantry.DomainEvents
{
    /// <summary>
    /// Represents a domain event.
    /// </summary>
    public interface IDomainEvent : IIdentifiable, ITimestamped
    {
    }
}
