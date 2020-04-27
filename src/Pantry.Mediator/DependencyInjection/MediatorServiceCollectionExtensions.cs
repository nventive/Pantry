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
    }
}
