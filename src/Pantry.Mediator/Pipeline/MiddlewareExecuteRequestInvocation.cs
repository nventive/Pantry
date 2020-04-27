using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pantry.Mediator.Pipeline
{
    /// <summary>
    /// Encapsulate middleware pipeline executions.
    /// </summary>
    internal class MiddlewareExecuteRequestInvocation : ExecuteRequestInvocation
    {
        private readonly IDomainRequestMiddleware _middleware;
        private readonly ExecuteRequestInvocation _nextInvocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareExecuteRequestInvocation"/> class.
        /// </summary>
        /// <param name="middleware">The current middleware to execute.</param>
        /// <param name="nextInvocation">The next invocation in the pipeline.</param>
        public MiddlewareExecuteRequestInvocation(IDomainRequestMiddleware middleware, ExecuteRequestInvocation nextInvocation)
        {
            _middleware = middleware ?? throw new ArgumentNullException(nameof(middleware));
            _nextInvocation = nextInvocation ?? throw new ArgumentNullException(nameof(nextInvocation));
        }

        /// <inheritdoc/>
        public override async Task<object> ProceedAsync(IDomainRequest request, CancellationToken cancellationToken)
        {
            return await _middleware
                .HandleAsync(
                    request,
                    (req) => _nextInvocation.ProceedAsync(req, cancellationToken),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
