using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        public static IServiceCollection TryRegisterAsSelfAndAllInterfaces<T>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            => services.TryRegisterAsSelfAndAllInterfaces(typeof(T), lifetime);

        /// <summary>
        /// Tries to register <paramref name="implementationType"/> as self and all its interfaces.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="implementationType">The implementation type to register.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryRegisterAsSelfAndAllInterfaces(
        this IServiceCollection services,
        Type implementationType,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
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
    }
}
