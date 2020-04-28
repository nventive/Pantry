using System;
using System.Collections;
using System.Text.Json;
using System.Threading.Tasks;
using FormatWith;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pantry.Continuation;

namespace Pantry.Mediator.AspNetCore.Execution
{
    /// <summary>
    /// <see cref="IDomainRequestExecutor"/> default implementation.
    /// </summary>
    public class DomainRequestExecutor : IDomainRequestExecutor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainRequestExecutor"/> class.
        /// </summary>
        /// <param name="mediator">The <see cref="IDynamicMediator"/>.</param>
        /// <param name="requestBinder">The <see cref="IDomainRequestBinder"/>.</param>
        /// <param name="jsonOptions">The <see cref="JsonOptions"/>.</param>
        public DomainRequestExecutor(
            IDynamicMediator mediator,
            IDomainRequestBinder requestBinder,
            IOptions<JsonOptions> jsonOptions)
        {
            Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            RequestBinder = requestBinder ?? throw new ArgumentNullException(nameof(requestBinder));
            JsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
        }

        /// <summary>
        /// Gets the <see cref="IDynamicMediator"/>.
        /// </summary>
        protected IDynamicMediator Mediator { get; }

        /// <summary>
        /// Gets the <see cref="IDomainRequestBinder"/>.
        /// </summary>
        protected IDomainRequestBinder RequestBinder { get; }

        /// <summary>
        /// Gets the <see cref="JsonOptions"/>.
        /// </summary>
        protected IOptions<JsonOptions> JsonOptions { get; }

        /// <inheritdoc/>
        public virtual async Task ExecuteAsync(HttpContext context, Type domainRequestType, DomainRequestExecutionOptions? options)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var domainRequest = await RequestBinder.Bind(context, domainRequestType);

            var result = await Mediator.ExecuteAsync(domainRequest, context.RequestAborted).ConfigureAwait(false);

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
                "POST" => string.IsNullOrEmpty(options?.CreatedAtRedirectPattern) ? StatusCodes.Status200OK : StatusCodes.Status201Created,
                "DELETE" => StatusCodes.Status204NoContent,
                _ => StatusCodes.Status200OK,
            };

            if (!string.IsNullOrEmpty(options?.CreatedAtRedirectPattern))
            {
                context.Response.GetTypedHeaders().Location = new Uri(options.CreatedAtRedirectPattern.FormatWith(result), UriKind.RelativeOrAbsolute);
            }

            if (context.Response.StatusCode != StatusCodes.Status204NoContent)
            {
                await JsonSerializer.SerializeAsync(
                    context.Response.Body,
                    result,
                    result.GetType(),
                    options: JsonOptions.Value.JsonSerializerOptions,
                    cancellationToken: context.RequestAborted)
                    .ConfigureAwait(false);
            }
        }
    }
}
