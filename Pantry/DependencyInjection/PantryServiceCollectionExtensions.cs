using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry.Generators;
using Pantry.Queries;

namespace Pantry.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class PantryServiceCollectionExtensions
    {
        /// <summary>
        /// Tries to register <typeparamref name="T"/> as self and all its interfaces.
        /// </summary>
        /// <typeparam name="T">The implementation to register.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddAsSelfAndAllInterfaces<T>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            => services.TryAddAsSelfAndAllInterfaces(typeof(T), lifetime);

        /// <summary>
        /// Tries to register <paramref name="implementationType"/> as self and all its interfaces.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="implementationType">The implementation type to register.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddAsSelfAndAllInterfaces(
            this IServiceCollection services,
            Type implementationType,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (implementationType is null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            services.TryAdd(new ServiceDescriptor(implementationType, implementationType, lifetime));
            foreach (var iface in implementationType.GetInterfaces())
            {
                services.TryAdd(new ServiceDescriptor(iface, sp => sp.GetService(implementationType), lifetime));
            }

            return services;
        }

        /// <summary>
        /// Adds <typeparamref name="T"/> as a query handler. The lifetime is <see cref="ServiceLifetime.Transient"/>.
        /// </summary>
        /// <typeparam name="T">The query handler type.</typeparam>
        /// <typeparam name="TQueryHandlerBaseType">The base handler type to register with.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddQueryHandler<T, TQueryHandlerBaseType>(this IServiceCollection services)
            => services.AddQueryHandler<TQueryHandlerBaseType>(typeof(T));

        /// <summary>
        /// Adds <paramref name="queryHandlerType"/> as a query handler. The lifetime is <see cref="ServiceLifetime.Transient"/>.
        /// </summary>
        /// <typeparam name="TQueryHandlerBaseType">The base handler type to register with.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="queryHandlerType">The query handler type.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddQueryHandler<TQueryHandlerBaseType>(this IServiceCollection services, Type queryHandlerType)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (queryHandlerType is null)
            {
                throw new ArgumentNullException(nameof(queryHandlerType));
            }

            services.Add(new ServiceDescriptor(queryHandlerType, queryHandlerType, ServiceLifetime.Transient));
            services.TryAddEnumerable(new ServiceDescriptor(typeof(TQueryHandlerBaseType), queryHandlerType, ServiceLifetime.Transient));

            return services;
        }

        /// <summary>
        /// Tries to add an <see cref="IIdGenerator{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddIdGeneratorFor<TEntity>(this IServiceCollection services)
            where TEntity : class
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IIdGenerator<TEntity>, GuidIdGenerator<TEntity>>();

            return services;
        }

        /// <summary>
        /// Tries to add an <see cref="IETagGenerator{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddETagGeneratorFor<TEntity>(this IServiceCollection services)
            where TEntity : class
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IETagGenerator<TEntity>, SHA1ETagGenerator<TEntity>>();

            return services;
        }
    }
}
