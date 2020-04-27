using System.Threading.Tasks;

namespace Pantry.Mediator
{
    /// <summary>
    /// Handles <see cref="IDomainEvent"/>.
    /// </summary>
    /// <typeparam name="TDomainEvent">The <see cref="IDomainEvent"/> type to handle.</typeparam>
    public interface IDomainEventHandler<TDomainEvent> : IDomainEventHandler
        where TDomainEvent : class, IDomainEvent
    {
        /// <summary>
        /// Handles the <paramref name="domainEvent"/>.
        /// </summary>
        /// <param name="domainEvent">The <see cref="IDomainEvent"/> to handle.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HandleAsync(TDomainEvent domainEvent);
    }
}
