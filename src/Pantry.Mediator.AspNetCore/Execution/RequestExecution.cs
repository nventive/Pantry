using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Pantry.Mediator.AspNetCore.Execution
{
    /// <summary>
    /// Manages the execution of <see cref="IDomainRequest"/> from HTTP.
    /// </summary>
    public static class RequestExecution
    {
        /// <summary>
        /// The request delegate.
        /// </summary>
        /// <typeparam name="TDomainRequest">The type of <see cref="IDomainRequest"/>.</typeparam>
        /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RequestDelegate<TDomainRequest>(HttpContext httpContext)
            where TDomainRequest : IDomainRequest, new()
        {
            if (httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var jsonSerializerOptions = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
            var mediator = httpContext.RequestServices.GetRequiredService<IDynamicMediator>();

            var domainRequest = httpContext.Request.Method.ToUpperInvariant() switch
            {
                "GET" => new TDomainRequest(),
                _ => await JsonSerializer.DeserializeAsync<TDomainRequest>(httpContext.Request.Body, jsonSerializerOptions),
            };

            BindRouteData(httpContext, domainRequest);

            var result = await mediator.ExecuteAsync(domainRequest, httpContext.RequestAborted).ConfigureAwait(false);

            httpContext.Response.StatusCode = httpContext.Request.Method.ToUpperInvariant() switch
            {
                "POST" => StatusCodes.Status201Created,
                "DELETE" => StatusCodes.Status204NoContent,
                _ => StatusCodes.Status200OK,
            };

            await JsonSerializer.SerializeAsync(httpContext.Response.Body, result, result.GetType(), options: jsonSerializerOptions, cancellationToken: httpContext.RequestAborted).ConfigureAwait(false);
        }

        private static void BindRouteData<TDomainRequest>(HttpContext httpContext, TDomainRequest domainRequest)
        {
            var allDomainRequestProperties = typeof(TDomainRequest).GetProperties()
                .Where(x => x.GetSetMethod() != null)
                .ToDictionary(x => x.Name.ToUpperInvariant());

            foreach (var routeValue in httpContext.Request.RouteValues)
            {
                if (allDomainRequestProperties.ContainsKey(routeValue.Key.ToUpperInvariant()))
                {
                    allDomainRequestProperties[routeValue.Key.ToUpperInvariant()].GetSetMethod() !.Invoke(domainRequest, new object[] { routeValue.Value });
                }
            }
        }
    }
}
