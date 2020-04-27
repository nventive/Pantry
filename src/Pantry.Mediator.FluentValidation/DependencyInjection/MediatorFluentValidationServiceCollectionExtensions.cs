using Pantry.Mediator;
using Pantry.Mediator.FluentValidation;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class MediatorFluentValidationServiceCollectionExtensions
    {
        /// <summary>
        /// Will perform validation using Fluent Validation on all incoming requests
        /// before the handler is invoked.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddFluentValidationRequestMiddleware(this IServiceCollection services)
        {
            services.AddTransient<IDomainRequestMiddleware, FluentValidationDomainRequestMiddleware>();
            return services;
        }
    }
}
