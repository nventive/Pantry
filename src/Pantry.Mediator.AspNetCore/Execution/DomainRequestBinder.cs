using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using Pantry.Continuation;
using Pantry.Mediator.AspNetCore.Models;

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

        /// <inheritdoc/>
        public ApiDescription GetApiDescriptionFor(Type domainRequestType, string pattern, string httpMethod, DomainRequestExecutionOptions? options)
        {
            if (domainRequestType is null)
            {
                throw new ArgumentNullException(nameof(domainRequestType));
            }

            if (httpMethod is null)
            {
                throw new ArgumentNullException(nameof(httpMethod));
            }

            var apiDescription = new ApiDescription
            {
                GroupName = options?.GroupName ?? domainRequestType.Name,
                RelativePath = pattern,
                HttpMethod = httpMethod,
            };

            apiDescription.SupportedRequestFormats.Add(new ApiRequestFormat
            {
                MediaType = "application/json",
            });

            apiDescription.ActionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = domainRequestType.GetTypeInfo(),
                MethodInfo = typeof(DummyControllerClass).GetMethod("Index"),
                DisplayName = domainRequestType.Name,
                ControllerName = apiDescription.GroupName,
                ActionName = "Execute",
            };

            var routeTokens = RoutePatternFactory.Parse(pattern);
            foreach (var parameter in routeTokens.Parameters)
            {
                apiDescription.ParameterDescriptions.Add(new ApiParameterDescription
                {
                    Name = parameter.Name,
                    Source = BindingSource.Path,
                });
            }

            switch (httpMethod.ToUpperInvariant())
            {
                case "GET":
                case "DELETE":
                    AddQueryDataParameters(apiDescription, domainRequestType, routeTokens.Parameters);
                    break;
                default:
                    AddBodySchema(apiDescription, domainRequestType, routeTokens.Parameters);
                    break;
            }

            AddResponses(apiDescription, domainRequestType, httpMethod, routeTokens.Parameters, options);

            return apiDescription;
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
                    SetProperty(
                        domainRequest,
                        allDomainRequestProperties[queryValue.Key.ToUpperInvariant()],
                        queryValue.Value.Count == 1 ? (object)queryValue.Value.ToString() : queryValue.Value);
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

        /// <summary>
        /// Adds the query string parameters to the <paramref name="apiDescription"/>.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription"/>.</param>
        /// <param name="domainRequestType">The <see cref="IDomainRequest"/> concrete type.</param>
        /// <param name="parameters">The route paramerters that already exists.</param>
        protected virtual void AddQueryDataParameters(ApiDescription apiDescription, Type domainRequestType, IEnumerable<RoutePatternParameterPart> parameters)
        {
            if (apiDescription is null)
            {
                throw new ArgumentNullException(nameof(apiDescription));
            }

            if (domainRequestType is null)
            {
                throw new ArgumentNullException(nameof(domainRequestType));
            }

            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var allProperties = GetDomainRequestPropertiesByName(domainRequestType);
            var propertyNamesAlreadyAdded = parameters.Select(x => x.Name.ToUpperInvariant()).ToHashSet();

            foreach (var property in allProperties.Where(x => !propertyNamesAlreadyAdded.Contains(x.Key)))
            {
                apiDescription.ParameterDescriptions.Add(new ApiParameterDescription
                {
                    Name = property.Value.Name,
                    Source = BindingSource.Query,
                    Type = property.Value.PropertyType,
                });
            }
        }

        /// <summary>
        /// Adds the body schema to the <paramref name="apiDescription"/>.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription"/>.</param>
        /// <param name="domainRequestType">The <see cref="IDomainRequest"/> concrete type.</param>
        /// <param name="parameters">The route paramerters that already exists.</param>
        protected virtual void AddBodySchema(ApiDescription apiDescription, Type domainRequestType, IEnumerable<RoutePatternParameterPart> parameters)
        {
            if (apiDescription is null)
            {
                throw new ArgumentNullException(nameof(apiDescription));
            }

            apiDescription.ParameterDescriptions.Add(new ApiParameterDescription
            {
                Source = BindingSource.Body,
                Name = "Body",
                Type = domainRequestType,
            });
        }

        /// <summary>
        /// Adds the OpenApi responses.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription"/>.</param>
        /// <param name="domainRequestType">The <see cref="IDomainRequest"/> type.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="routeTokens">The route tokens.</param>
        /// <param name="options">The options, if any.</param>
        protected virtual void AddResponses(ApiDescription apiDescription, Type domainRequestType, string httpMethod, IReadOnlyList<RoutePatternParameterPart> routeTokens, DomainRequestExecutionOptions? options)
        {
            if (apiDescription is null)
            {
                throw new ArgumentNullException(nameof(apiDescription));
            }

            if (domainRequestType is null)
            {
                throw new ArgumentNullException(nameof(domainRequestType));
            }

            if (httpMethod is null)
            {
                throw new ArgumentNullException(nameof(httpMethod));
            }

            var responseType = domainRequestType
                .GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDomainRequest<>))
                .GetGenericArguments()[0];

            var continuationEnumerableResponseType = responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(IContinuationEnumerable<>)
                ? responseType
                : responseType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IContinuationEnumerable<>));

            if (continuationEnumerableResponseType != null)
            {
                responseType = typeof(ContinuationEnumerableModel<>).MakeGenericType(continuationEnumerableResponseType.GetGenericArguments()[0]);
            }

            apiDescription.SupportedResponseTypes.Add(new ApiResponseType
            {
                StatusCode = DomainRequestExecutor.GetStatusCode(httpMethod, options),
                ApiResponseFormats = new List<ApiResponseFormat> { new ApiResponseFormat { MediaType = "application/json" } },
                Type = responseType,
            });

            if (routeTokens.Any(x => x.Name.ToUpperInvariant() == "ID"))
            {
                apiDescription.SupportedResponseTypes.Add(new ApiResponseType
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    ApiResponseFormats = new List<ApiResponseFormat> { new ApiResponseFormat { MediaType = "application/json" } },
                    Type = typeof(ProblemDetails),
                });
            }
            else
            {
                if (httpMethod.ToUpperInvariant() == "POST")
                {
                    apiDescription.SupportedResponseTypes.Add(new ApiResponseType
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        ApiResponseFormats = new List<ApiResponseFormat> { new ApiResponseFormat { MediaType = "application/json" } },
                        Type = typeof(ProblemDetails),
                    });
                }
            }

            if (new[] { "POST", "PUT", "PATCH" }.Contains(httpMethod.ToUpperInvariant()))
            {
                apiDescription.SupportedResponseTypes.Add(new ApiResponseType
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ApiResponseFormats = new List<ApiResponseFormat> { new ApiResponseFormat { MediaType = "application/json" } },
                    Type = typeof(ValidationProblemDetails),
                });

                if (httpMethod.ToUpperInvariant() != "POST")
                {
                    apiDescription.SupportedResponseTypes.Add(new ApiResponseType
                    {
                        StatusCode = StatusCodes.Status412PreconditionFailed,
                        ApiResponseFormats = new List<ApiResponseFormat> { new ApiResponseFormat { MediaType = "application/json" } },
                        Type = typeof(ProblemDetails),
                    });
                }
            }

            apiDescription.SupportedResponseTypes.Add(new ApiResponseType
            {
                IsDefaultResponse = true,
                ApiResponseFormats = new List<ApiResponseFormat> { new ApiResponseFormat { MediaType = "application/json" } },
                Type = typeof(ProblemDetails),
            });
        }

        private static Dictionary<string, PropertyInfo> GetDomainRequestPropertiesByName(Type domainRequestType)
            => domainRequestType.GetProperties()
                .Where(x => x.GetSetMethod() != null)
                .ToDictionary(x => x.Name.ToUpperInvariant());

        private static void SetProperty(IDomainRequest domainRequest, PropertyInfo targetProperty, object value)
        {
            var enumerablePropertyType = targetProperty.PropertyType.IsGenericType && targetProperty.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            if (enumerablePropertyType)
            {
                var enumeratedType = targetProperty.PropertyType.GetGenericArguments()[0];
                var typeConverter = TypeDescriptor.GetConverter(enumeratedType);
                // Container for the enumeration
                var targetValue = value is IEnumerable enumerable && !(value is string)
                    ? enumerable.Cast<object>().Select(x => typeConverter.ConvertFrom(x)).ToArray()
                    : new object[] { typeConverter.ConvertFrom(value) };
                var finalValue = Array.CreateInstance(enumeratedType, targetValue.Length);
                targetValue.CopyTo(finalValue, 0);
                targetProperty.GetSetMethod() !.Invoke(domainRequest, new object[] { finalValue });
            }
            else
            {
                var typeConverter = TypeDescriptor.GetConverter(targetProperty.PropertyType);
                var targetValue = value is string valueStr
                     ? typeConverter.ConvertFromString(valueStr)
                     : typeConverter.ConvertFrom(value);
                targetProperty.GetSetMethod() !.Invoke(domainRequest, new object[] { targetValue });
            }
        }

#pragma warning disable CA1812
        private class DummyControllerClass
        {
            public IActionResult Index()
            {
                throw new NotImplementedException();
            }
        }
    }
}
