using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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
        /// GET HTTP Verb.
        /// </summary>
        public static readonly IEnumerable<string> GetVerb = new[] { "GET" };

        /// <summary>
        /// POST HTTP Verb.
        /// </summary>
        public static readonly IEnumerable<string> PostVerb = new[] { "POST" };

        /// <summary>
        /// PUT HTTP Verb.
        /// </summary>
        public static readonly IEnumerable<string> PutVerb = new[] { "PUT" };

        /// <summary>
        /// PATCH HTTP Verb.
        /// </summary>
        public static readonly IEnumerable<string> PatchVerb = new[] { "PATCH" };

        /// <summary>
        /// DELETE HTTP Verb.
        /// </summary>
        public static readonly IEnumerable<string> DeleteVerb = new[] { "DELETE" };

        /// <summary>
        /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP requests for the specified HTTP methods and pattern
        /// and executes the <paramref name="domainRequestType"/> on the <see cref="IMediator"/>.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="domainRequestType">The <see cref="IDomainRequest"/> type.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="httpMethods">HTTP methods that the endpoint will match.</param>
        /// <param name="options">THe <see cref="DomainRequestExecutionOptions"/>.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static IEndpointRouteBuilder MapMethods(
            this IEndpointRouteBuilder endpoints,
            Type domainRequestType,
            string pattern,
            IEnumerable<string> httpMethods,
            DomainRequestExecutionOptions? options = null)
        {
            endpoints.MapMethods(
                pattern,
                httpMethods,
                async context =>
                {
                    var executor = context.RequestServices.GetRequiredService<IDomainRequestExecutor>();
                    await executor.ExecuteAsync(context, domainRequestType, options).ConfigureAwait(false);
                });
            return endpoints;
        }

        /// <summary>
        /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP requests for the specified HTTP methods and pattern
        /// and executes the <typeparamref name="TDomainRequest"/> on the <see cref="IMediator"/>.
        /// </summary>
        /// <typeparam name="TDomainRequest">The type of <see cref="IDomainRequest"/>.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="httpMethods">HTTP methods that the endpoint will match.</param>
        /// <param name="options">THe <see cref="DomainRequestExecutionOptions"/>.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static IEndpointRouteBuilder MapMethods<TDomainRequest>(this IEndpointRouteBuilder endpoints, string pattern, IEnumerable<string> httpMethods, DomainRequestExecutionOptions? options = null)
            where TDomainRequest : IDomainRequest, new()
            => endpoints.MapMethods(typeof(TDomainRequest), pattern, httpMethods, options: options);

        /// <summary>
        /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP GET requests for the specified pattern,
        /// and executes the <typeparamref name="TDomainRequest"/> on the <see cref="IMediator"/>.
        /// </summary>
        /// <typeparam name="TDomainRequest">The type of <see cref="IDomainRequest"/>.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static IEndpointRouteBuilder MapGet<TDomainRequest>(this IEndpointRouteBuilder endpoints, string pattern)
            where TDomainRequest : IDomainRequest, new()
            => endpoints.MapMethods<TDomainRequest>(pattern, GetVerb);

        /// <summary>
        /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP POST requests for the specified pattern,
        /// and executes the <typeparamref name="TDomainRequest"/> on the <see cref="IMediator"/>.
        /// </summary>
        /// <typeparam name="TDomainRequest">The type of <see cref="IDomainRequest"/>.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="createdAtRedirectPattern">When specified, the execution will return 201 CreatedAt with the location pattern.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static IEndpointRouteBuilder MapPost<TDomainRequest>(this IEndpointRouteBuilder endpoints, string pattern, string? createdAtRedirectPattern = null)
            where TDomainRequest : IDomainRequest, new()
            => endpoints.MapMethods<TDomainRequest>(pattern, PostVerb, new DomainRequestExecutionOptions { CreatedAtRedirectPattern = createdAtRedirectPattern });

        /// <summary>
        /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP PUT requests for the specified pattern,
        /// and executes the <typeparamref name="TDomainRequest"/> on the <see cref="IMediator"/>.
        /// </summary>
        /// <typeparam name="TDomainRequest">The type of <see cref="IDomainRequest"/>.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static IEndpointRouteBuilder MapPut<TDomainRequest>(this IEndpointRouteBuilder endpoints, string pattern)
            where TDomainRequest : IDomainRequest, new()
            => endpoints.MapMethods<TDomainRequest>(pattern, PutVerb);

        /// <summary>
        /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP PATCH requests for the specified pattern,
        /// and executes the <typeparamref name="TDomainRequest"/> on the <see cref="IMediator"/>.
        /// </summary>
        /// <typeparam name="TDomainRequest">The type of <see cref="IDomainRequest"/>.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static IEndpointRouteBuilder MapPatch<TDomainRequest>(this IEndpointRouteBuilder endpoints, string pattern)
            where TDomainRequest : IDomainRequest, new()
            => endpoints.MapMethods<TDomainRequest>(pattern, PatchVerb);

        /// <summary>
        /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP DELETE requests for the specified pattern,
        /// and executes the <typeparamref name="TDomainRequest"/> on the <see cref="IMediator"/>.
        /// </summary>
        /// <typeparam name="TDomainRequest">The type of <see cref="IDomainRequest"/>.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static IEndpointRouteBuilder MapDelete<TDomainRequest>(this IEndpointRouteBuilder endpoints, string pattern)
            where TDomainRequest : IDomainRequest, new()
            => endpoints.MapMethods<TDomainRequest>(pattern, DeleteVerb);
    }
}
