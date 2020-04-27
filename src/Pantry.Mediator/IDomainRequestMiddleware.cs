using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pantry.Mediator
{
    /// <summary>
    /// A middleware that can be excuted as part of the mediator pipeline.
    /// </summary>
    public interface IDomainRequestMiddleware
    {
        /// <summary>
        /// Called during the pipeline execution. Receives the <paramref name="request"/>
        /// and can either proceed with th execution using <paramref name="nextHandler"/>, or short-circuit if needed.
        /// </summary>
        /// <param name="request">The current request.</param>
        /// <param name="nextHandler">The next pipeline executor.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The request result.</returns>
        Task<object> HandleAsync(IDomainRequest request, Func<IDomainRequest, Task<object>> nextHandler, CancellationToken cancellationToken);
    }
}
