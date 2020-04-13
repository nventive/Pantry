using System.Threading.Tasks;

namespace Pantry.DomainEvents
{
    /// <summary>
    /// Allows the dispatch of <see cref="IDomainEvent"/>.
    /// </summary>
    public interface IDomainEventsDispatcher
    {
        /// <summary>
        /// Dispatch a <paramref name="domainEvent"/>.
        /// </summary>
        /// <param name="domainEvent">The <see cref="IDomainEvent"/> to dispatch.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DispatchAsync(IDomainEvent domainEvent);
    }
}
