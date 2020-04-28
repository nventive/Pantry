using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pantry.Continuation;

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
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RequestDelegate<TDomainRequest>(HttpContext context)
            where TDomainRequest : IDomainRequest, new()
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var jsonSerializerOptions = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
            var mediator = context.RequestServices.GetRequiredService<IDynamicMediator>();

            var domainRequest = context.Request.Method.ToUpperInvariant() switch
            {
                "GET" => BindQueryData<TDomainRequest>(context),
                _ => await JsonSerializer.DeserializeAsync<TDomainRequest>(context.Request.Body, jsonSerializerOptions),
            };

            BindRouteData(context, domainRequest);

            var result = await mediator.ExecuteAsync(domainRequest, context.RequestAborted).ConfigureAwait(false);

            if (result is IContinuation continuation && result is IEnumerable enumerable)
            {
                result = new
                {
                    continuationToken = continuation.ContinuationToken,
                    items = enumerable,
                };
            }

            context.Response.StatusCode = context.Request.Method.ToUpperInvariant() switch
            {
                "POST" => StatusCodes.Status201Created,
                "DELETE" => StatusCodes.Status204NoContent,
                _ => StatusCodes.Status200OK,
            };

            if (context.Response.StatusCode != StatusCodes.Status204NoContent)
            {
                await JsonSerializer.SerializeAsync(context.Response.Body, result, result.GetType(), options: jsonSerializerOptions, cancellationToken: context.RequestAborted).ConfigureAwait(false);
            }
        }

        private static TDomainRequest BindQueryData<TDomainRequest>(HttpContext context)
            where TDomainRequest : IDomainRequest, new()
        {
            var domainRequest = new TDomainRequest();

            var allDomainRequestProperties = typeof(TDomainRequest).GetProperties()
                .Where(x => x.GetSetMethod() != null)
                .ToDictionary(x => x.Name.ToUpperInvariant());

            foreach (var queryValue in context.Request.Query)
            {
                if (allDomainRequestProperties.ContainsKey(queryValue.Key.ToUpperInvariant()))
                {
                    var targetProperty = allDomainRequestProperties[queryValue.Key.ToUpperInvariant()];
                    var typeConverter = TypeDescriptor.GetConverter(targetProperty.PropertyType);
                    var targetValue = typeConverter.ConvertFromString(queryValue.Value);
                    targetProperty.GetSetMethod() !.Invoke(domainRequest, new object[] { targetValue });
                }
            }

            return domainRequest;
        }

        private static void BindRouteData<TDomainRequest>(HttpContext context, TDomainRequest domainRequest)
        {
            var allDomainRequestProperties = typeof(TDomainRequest).GetProperties()
                .Where(x => x.GetSetMethod() != null)
                .ToDictionary(x => x.Name.ToUpperInvariant());

            foreach (var routeValue in context.Request.RouteValues)
            {
                if (allDomainRequestProperties.ContainsKey(routeValue.Key.ToUpperInvariant()))
                {
                    var targetProperty = allDomainRequestProperties[routeValue.Key.ToUpperInvariant()];
                    var typeConverter = TypeDescriptor.GetConverter(targetProperty.PropertyType);
                    var targetValue = typeConverter.ConvertFrom(routeValue.Value);
                    allDomainRequestProperties[routeValue.Key.ToUpperInvariant()].GetSetMethod() !.Invoke(domainRequest, new object[] { targetValue });
                }
            }
        }
    }
}
