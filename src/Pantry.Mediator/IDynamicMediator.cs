using System.Threading;
using System.Threading.Tasks;

namespace Pantry.Mediator
{
    /// <summary>
    /// This is the un-typed version of <see cref="IMediator"/>.
    /// Always consider using <see cref="IMediator"/> instead of this one.
    /// </summary>
    public interface IDynamicMediator
    {
        /// <summary>
        /// Execute the <paramref name="request"/> and return the result.
        /// </summary>
        /// <param name="request">The <see cref="IDomainRequest{TResult}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result.</returns>
        Task<object> ExecuteAsync(IDomainRequest request, CancellationToken cancellationToken = default);
    }
}
