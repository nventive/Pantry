using System.Threading;
using System.Threading.Tasks;

namespace Pantry.Mediator
{
    /// <summary>
    /// A mediator that allows execution of Queries and Commands, and also the publishing of Events
    /// throughout the application domain.
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Execute the <paramref name="request"/> and return the result.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="request">The <see cref="IDomainRequest{TResult}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result.</returns>
        Task<TResult> ExecuteAsync<TResult>(IDomainRequest<TResult> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publish a domain event.
        /// </summary>
        /// <param name="domainEvent">The event to publish.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task PublishAsync(IDomainEvent domainEvent);
    }
}
