using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Mediator.Exceptions;
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
        /// <param name="timestampProvider">The <see cref="ITimestampProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ServiceProviderMediator(
            IServiceProvider serviceProvider,
            ITimestampProvider timestampProvider,
            ILogger<ServiceProviderMediator>? logger = null)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            TimestampProvider = timestampProvider ?? throw new ArgumentNullException(nameof(timestampProvider));
            Logger = logger ?? NullLogger<ServiceProviderMediator>.Instance;
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected ITimestampProvider TimestampProvider { get; }

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
            var handler = ServiceProvider.GetService(handlerInterfaceType);
            if (handler is null)
            {
                Logger.LogWarning("No hander for {@Request}", request);
                throw new MediatorException($"Unable to find a handler for {request}.");
            }

            var handleMethod = handler.GetType().GetInterfaceMap(handlerInterfaceType).TargetMethods.FirstOrDefault(x => x.Name == "HandleAsync");
            if (handleMethod is null)
            {
                Logger.LogWarning("No hander method for {@Request} in {HandlerType} for {HandlerInterfaceType}", request, handler.GetType(), handlerInterfaceType);
                throw new MediatorException($"Unable to find proper handle method in {handler.GetType()} for {request}.");
            }

            var result = await ((Task<TResult>)handleMethod.Invoke(handler, new object[] { request, cancellationToken })).ConfigureAwait(false);

            Logger.LogTrace("Executed {@Request} by {Handler} -> {@Result}", request, handler, result);

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task PublishAsync(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            if (domainEvent.Timestamp is null)
            {
                domainEvent.Timestamp = TimestampProvider.CurrentTimestamp();
            }

            var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());

            var handlers = ServiceProvider.GetServices(handlerInterfaceType);
            foreach (var handler in handlers)
            {
                var handleMethod = handler.GetType().GetInterfaceMap(handlerInterfaceType).TargetMethods.FirstOrDefault(x => x.Name == "HandleAsync");
                if (handleMethod is null)
                {
                    Logger.LogWarning("No hander method for {@DomainEvent} in {HandlerType} for {HandlerInterfaceType}", domainEvent, handler.GetType(), handlerInterfaceType);
                    throw new MediatorException($"Unable to find proper handle method in {handler.GetType()}.");
                }

                await ((Task)handleMethod.Invoke(handler, new object[] { domainEvent })).ConfigureAwait(false);
                Logger.LogTrace("Executed {@DomainEvent} by {Handler}", domainEvent, handler);
            }
        }
    }
}
