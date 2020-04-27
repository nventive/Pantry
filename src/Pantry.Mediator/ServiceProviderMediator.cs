using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Mediator.Exceptions;
using Pantry.Mediator.Pipeline;
using Pantry.Providers;

namespace Pantry.Mediator
{
    /// <summary>
    /// <see cref="IMediator"/> implementation that uses a <see cref="IServiceProvider"/>.
    /// </summary>
    public class ServiceProviderMediator : IMediator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderMediator"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ServiceProviderMediator(
            IServiceProvider serviceProvider,
            ILogger<ServiceProviderMediator>? logger = null)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Logger = logger ?? NullLogger<ServiceProviderMediator>.Instance;
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected ITimestampProvider TimestampProvider => ServiceProvider.GetRequiredService<ITimestampProvider>();

        /// <summary>
        /// Gets the <see cref="IDomainRequestMiddleware"/> in reverse order.
        /// </summary>
        protected IEnumerable<IDomainRequestMiddleware> ReversedRequestsMiddlewares => ServiceProvider.GetServices<IDomainRequestMiddleware>().Reverse();

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc/>
        public virtual async Task<TResult> ExecuteAsync<TResult>(IDomainRequest<TResult> request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var handlerInterfaceType = typeof(IDomainRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResult));
            if (!(ServiceProvider.GetService(handlerInterfaceType) is IDomainRequestHandler handler))
            {
                Logger.LogWarning("No hander for {@Request} - Looking for {HandlerInterfaceType}", request, handlerInterfaceType);
                throw new MediatorException($"Unable to find a handler for {request} using {handlerInterfaceType}.");
            }

            var invocation = ReversedRequestsMiddlewares.Aggregate(
                (ExecuteRequestInvocation)new HandlerExecuteRequestInvocation(handler, handlerInterfaceType),
                (agg, value) => new MiddlewareExecuteRequestInvocation(value, agg));

            Stopwatch? stopWatch = null;
            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTrace("Executing {@Request} by {Handler}", request, handler);
                stopWatch = Stopwatch.StartNew();
            }

            var result = await invocation.ProceedAsync(request, cancellationToken).ConfigureAwait(false);

            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTrace("Executed {@Request} by {Handler} -> {@Result} in {Elapsed} ms.", request, handler, result, stopWatch!.ElapsedMilliseconds);
            }

            return (TResult)result;
        }

        /// <inheritdoc/>
        public virtual async Task PublishAsync(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            Logger.LogTrace("Publishing {@DomainEvent}", domainEvent);

            if (domainEvent.Timestamp is null)
            {
                domainEvent.Timestamp = TimestampProvider.CurrentTimestamp();
            }

            var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());

            var handlers = ServiceProvider.GetServices(handlerInterfaceType);
            foreach (var handler in handlers)
            {
                var handleMethod = handler.GetType().GetInterfaceMap(handlerInterfaceType).TargetMethods.First(x => x.Name == "HandleAsync");

                Stopwatch? stopWatch = null;
                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    stopWatch = Stopwatch.StartNew();
                }

                await ((Task)handleMethod.Invoke(handler, new object[] { domainEvent })).ConfigureAwait(false);

                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    Logger.LogTrace("Executed {@DomainEvent} by {Handler} in {Elapsed} ms.", domainEvent, handler, stopWatch!.ElapsedMilliseconds);
                }
            }
        }
    }
}
