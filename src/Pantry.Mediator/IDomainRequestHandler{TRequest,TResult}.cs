using System.Threading;
using System.Threading.Tasks;

namespace Pantry.Mediator
{
    /// <summary>
    /// Handles <see cref="IDomainRequest{TResult}"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    public interface IDomainRequestHandler<in TRequest, TResult>
        where TRequest : IDomainRequest<TResult>
    {
        /// <summary>
        /// Handle the <paramref name="request"/> and returns the result.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result.</returns>
        Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
