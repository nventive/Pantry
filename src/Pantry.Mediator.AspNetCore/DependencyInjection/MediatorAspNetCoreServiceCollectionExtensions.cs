using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry.Mediator;
using Pantry.Mediator.AspNetCore.ApiExplorer;
using Pantry.Mediator.AspNetCore.Execution;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class MediatorAspNetCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services necessary for the execution and binding of <see cref="IDomainRequest"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDomainRequestRestApiExecution(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<DomainRequestApiDescriptionContainer>();
            services.TryAddSingleton<IDomainRequestBinder, DomainRequestBinder>();
            services.TryAddTransient<IDomainRequestExecutor, DomainRequestExecutor>();
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IApiDescriptionProvider), typeof(DomainRequestApiDescriptionProvider), ServiceLifetime.Transient));

            return services;
        }
    }
}
