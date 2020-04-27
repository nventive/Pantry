using System.Threading;
using System.Threading.Tasks;

namespace Pantry.Mediator.Invocations
{
    /// <summary>
    /// Encapsulates pipeline invocations.
    /// </summary>
    internal abstract class ExecuteRequestInvocation
    {
        /// <summary>
        /// Proceed with the pipeline invocation.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">THe <see cref="CancellationToken"/>.</param>
        /// <returns>The result.</returns>
        public abstract Task<object> ProceedAsync(IDomainRequest request, CancellationToken cancellationToken);
    }
}
