using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry.Continuation;
using Pantry.Generators;
using Pantry.Providers;

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
        /// <returns>The registered interfaces types.</returns>
        public static IEnumerable<Type> TryAddAsSelfAndAllInterfaces<T>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            => services.TryAddAsSelfAndAllInterfaces(typeof(T), lifetime);

        /// <summary>
        /// Tries to register <paramref name="implementationType"/> as self and all its interfaces.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="implementationType">The implementation type to register.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
        /// <returns>The registered interfaces types.</returns>
        public static IEnumerable<Type> TryAddAsSelfAndAllInterfaces(
            this IServiceCollection services,
            Type implementationType,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (implementationType is null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            var result = new List<Type>();

            services.TryAdd(new ServiceDescriptor(implementationType, implementationType, lifetime));
            foreach (var iface in implementationType.GetInterfaces())
            {
                services.TryAdd(new ServiceDescriptor(iface, sp => sp.GetService(implementationType), lifetime));
                result.Add(iface);
            }

            return result;
        }

        /// <summary>
        /// Tries to add an <see cref="ITimestampProvider"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddTimestampProvider(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<ITimestampProvider, SystemClockTimestampProvider>();

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

        /// <summary>
        /// Tries to add an <see cref="IContinuationTokenEncoder{TContinuationToken}"/>.
        /// </summary>
        /// <typeparam name="TContinuationToken">The continuation token type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddContinuationTokenEncoderFor<TContinuationToken>(this IServiceCollection services)
            where TContinuationToken : class
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IContinuationTokenEncoder<TContinuationToken>, Base64JsonContinuationTokenEncoder<TContinuationToken>>();

            return services;
        }
    }
}
