using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pantry.Exceptions;

namespace Pantry.DomainEvents
{
    /// <summary>
    /// <see cref="IDomainEventsDispatcher"/> implementation using <see cref="IServiceProvider"/>.
    /// </summary>
    public class ServiceProviderDomainEventsDispatcher : IDomainEventsDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderDomainEventsDispatcher"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public ServiceProviderDomainEventsDispatcher(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public async Task DispatchAsync(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
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
            }
        }
    }
}
