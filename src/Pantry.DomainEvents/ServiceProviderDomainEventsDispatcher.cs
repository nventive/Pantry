using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Exceptions;
using Pantry.Generators;
using Pantry.Providers;

namespace Pantry.DomainEvents
{
    /// <summary>
    /// <see cref="IDomainEventsDispatcher"/> implementation using <see cref="IServiceProvider"/>.
    /// </summary>
    public class ServiceProviderDomainEventsDispatcher : IDomainEventsDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IIdGenerator<IDomainEvent> _idGenerator;
        private readonly ITimestampProvider _timestampProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderDomainEventsDispatcher"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="idGenerator">The <see cref="IIdGenerator{IDomainEvent}"/>.</param>
        /// <param name="timestampProvider">The <see cref="ITimestampProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ServiceProviderDomainEventsDispatcher(
            IServiceProvider serviceProvider,
            IIdGenerator<IDomainEvent> idGenerator,
            ITimestampProvider timestampProvider,
            ILogger<ServiceProviderDomainEventsDispatcher>? logger = null)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _timestampProvider = timestampProvider ?? throw new ArgumentNullException(nameof(timestampProvider));
            _logger = logger ?? NullLogger<ServiceProviderDomainEventsDispatcher>.Instance;
        }

        /// <inheritdoc/>
        public async Task DispatchAsync(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            if (string.IsNullOrEmpty(domainEvent.Id))
            {
                domainEvent.Id = await _idGenerator.Generate(domainEvent);
            }

            if (domainEvent.Timestamp is null)
            {
                domainEvent.Timestamp = _timestampProvider.CurrentTimestamp();
            }

            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());

            var handlers = _serviceProvider.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                var handleMethod = handlerType.GetMethod("HandleAsync", new[] { domainEvent.GetType() });
                if (handleMethod is null)
                {
                    throw new InternalErrorException($"Unable to find proper handle method in {handlerType}.");
                }

                await ((Task)handleMethod.Invoke(handler, new object[] { domainEvent })).ConfigureAwait(false);
                _logger.LogDebug("Executed handler {Handler} for {DomainEvent}", handler, domainEvent);
            }
        }
    }
}
