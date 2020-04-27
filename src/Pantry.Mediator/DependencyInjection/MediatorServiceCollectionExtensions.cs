using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry.DependencyInjection;
using Pantry.Mediator;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class MediatorServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Mediator services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            services.TryAddTimestampProvider();
            services.TryAddTransient<IMediator, ServiceProviderMediator>();
            return services;
        }

        /// <summary>
        /// Registers all handler types (<see cref="IDomainEventHandler{TDomainEvent}"/> and <see cref="IDomainRequestHandler{TRequest, TResult}"/>)
        /// in the <paramref name="assembly"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assembly">The assembly to scan for.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDomainHandlersInAssembly(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (services is null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            if (assembly is null)
            {
                throw new System.ArgumentNullException(nameof(assembly));
            }

            var handlerTypes = assembly.GetTypes()
                .Where(x => !x.IsAbstract && !x.IsGenericTypeDefinition)
                .Where(x => typeof(IDomainRequestHandler).IsAssignableFrom(x) || typeof(IDomainEventHandler).IsAssignableFrom(x));

            foreach (var handlerType in handlerTypes)
            {
                services.Add(new ServiceDescriptor(handlerType, handlerType, lifetime));
                foreach (var iface in handlerType.GetInterfaces())
                {
                    services.Add(new ServiceDescriptor(iface, sp => sp.GetService(handlerType), lifetime));
                }
            }

            return services;
        }

        /// <summary>
        /// Registers all handler types (<see cref="IDomainEventHandler{TDomainEvent}"/> and <see cref="IDomainRequestHandler{TRequest, TResult}"/>)
        /// in the assembly containing <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type that is in the assebmly to scan.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDomainHandlersInAssemblyContaining<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
            => services.AddDomainHandlersInAssembly(typeof(T).Assembly, lifetime: lifetime);
    }
}
