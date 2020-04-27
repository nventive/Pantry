using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pantry.Mediator.Invocations
{
    /// <summary>
    /// Encapsulates pipeline executions for final handler.
    /// </summary>
    internal class HandlerExecuteRequestInvocation : ExecuteRequestInvocation
    {
        private readonly IDomainRequestHandler _handler;
        private readonly Type _handlerInterfaceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerExecuteRequestInvocation"/> class.
        /// </summary>
        /// <param name="handler">The handler to invoke.</param>
        /// <param name="handlerInterfaceType">The handler interface type.</param>
        public HandlerExecuteRequestInvocation(IDomainRequestHandler handler, Type handlerInterfaceType)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _handlerInterfaceType = handlerInterfaceType ?? throw new ArgumentNullException(nameof(handlerInterfaceType));
        }

        /// <inheritdoc/>
        public override async Task<object> ProceedAsync(IDomainRequest request, CancellationToken cancellationToken)
        {
            var handleMethod = _handler.GetType().GetInterfaceMap(_handlerInterfaceType).TargetMethods.First(x => x.Name == "HandleAsync");
            var task = (Task)handleMethod.Invoke(_handler, new object[] { request, cancellationToken });
            await task.ConfigureAwait(false);
            return (object)((dynamic)task).Result;
        }
    }
}
