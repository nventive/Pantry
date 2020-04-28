using Microsoft.AspNetCore.Routing;
using Pantry.Mediator;
using Pantry.Mediator.AspNetCore.Execution;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// <see cref="IEndpointRouteBuilder"/> extensions.
    /// </summary>
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Maps <typeparamref name="TDomainRequest"/> to the route <paramref name="pattern"/> and <paramref name="httpMethod"/>.
        /// </summary>
        /// <typeparam name="TDomainRequest">The type of <see cref="IDomainRequest"/>.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="httpMethod">HTTP method that the endpoint will match.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static IEndpointRouteBuilder MapDomainRequest<TDomainRequest>(this IEndpointRouteBuilder endpoints, string pattern, string httpMethod)
            where TDomainRequest : IDomainRequest, new()
        {
            endpoints.MapMethods(
                pattern,
                new[] { httpMethod },
                RequestExecution.RequestDelegate<TDomainRequest>);
            return endpoints;
        }
    }
}
