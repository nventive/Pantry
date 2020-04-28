using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Pantry.Mediator.AspNetCore.Execution
{
    /// <summary>
    /// <see cref="IDomainRequestBinder"/> default implementation.
    /// </summary>
    public class DomainRequestBinder : IDomainRequestBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainRequestBinder"/> class.
        /// </summary>
        /// <param name="jsonOptions">The <see cref="JsonOptions"/>.</param>
        public DomainRequestBinder(
            IOptions<JsonOptions> jsonOptions)
        {
            JsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
        }

        /// <summary>
        /// Gets the <see cref="JsonOptions"/>.
        /// </summary>
        protected IOptions<JsonOptions> JsonOptions { get; }

        /// <inheritdoc/>
        public async ValueTask<IDomainRequest> Bind(HttpContext context, Type domainRequestType)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var domainRequest = context.Request.Method.ToUpperInvariant() switch
            {
                "GET" => await BindQueryData(context, domainRequestType),
                "DELETE" => await BindQueryData(context, domainRequestType),
                _ => await BindBodyData(context, domainRequestType),
            };

            await BindRouteData(context, domainRequest);

            return domainRequest;
        }

        /// <summary>
        /// Binds the query data in <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="domainRequestType">The <see cref="IDomainRequest"/> concrete type.</param>
        /// <returns>The <see cref="IDomainRequest"/>.</returns>
        protected virtual async ValueTask<IDomainRequest> BindQueryData(HttpContext context, Type domainRequestType)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var domainRequest = (IDomainRequest)Activator.CreateInstance(domainRequestType) !;

            var allDomainRequestProperties = GetDomainRequestPropertiesByName(domainRequest.GetType());

            foreach (var queryValue in context.Request.Query)
            {
                if (allDomainRequestProperties.ContainsKey(queryValue.Key.ToUpperInvariant()))
                {
                    SetProperty(domainRequest, allDomainRequestProperties[queryValue.Key.ToUpperInvariant()], queryValue.Value.ToString());
                }
            }

            return domainRequest;
        }

        /// <summary>
        /// Binds the body data in <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="domainRequestType">The <see cref="IDomainRequest"/> concrete type.</param>
        /// <returns>The <see cref="IDomainRequest"/>.</returns>
        protected virtual async ValueTask<IDomainRequest> BindBodyData(HttpContext context, Type domainRequestType)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IDomainRequest? result = null;
            if (context.Request.Body != null)
            {
                result = (IDomainRequest?)await JsonSerializer.DeserializeAsync(context.Request.Body, domainRequestType, JsonOptions.Value.JsonSerializerOptions);
            }

            return result ?? (IDomainRequest)Activator.CreateInstance(domainRequestType) !;
        }

        /// <summary>
        /// Binds the route data in <paramref name="context"/> to the <paramref name="domainRequest"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="domainRequest">The <see cref="IDomainRequest"/>.</param>
        /// <returns>A ValueTask that can be awaited.</returns>
        protected virtual async ValueTask BindRouteData(HttpContext context, IDomainRequest domainRequest)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (domainRequest is null)
            {
                throw new ArgumentNullException(nameof(domainRequest));
            }

            var allDomainRequestProperties = GetDomainRequestPropertiesByName(domainRequest.GetType());

            foreach (var routeValue in context.Request.RouteValues)
            {
                if (allDomainRequestProperties.ContainsKey(routeValue.Key.ToUpperInvariant()))
                {
                    SetProperty(domainRequest, allDomainRequestProperties[routeValue.Key.ToUpperInvariant()], routeValue.Value);
                }
            }
        }

        private static Dictionary<string, PropertyInfo> GetDomainRequestPropertiesByName(Type domainRequestType)
            => domainRequestType.GetProperties()
                .Where(x => x.GetSetMethod() != null)
                .ToDictionary(x => x.Name.ToUpperInvariant());

        private static void SetProperty(IDomainRequest domainRequest, PropertyInfo targetProperty, object value)
        {
            var typeConverter = TypeDescriptor.GetConverter(targetProperty.PropertyType);
            var targetValue = value is string valueStr
                 ? typeConverter.ConvertFromString(valueStr)
                 : typeConverter.ConvertFrom(value);
            targetProperty.GetSetMethod() !.Invoke(domainRequest, new object[] { targetValue });
        }
    }
}
