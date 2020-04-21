using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Pantry.AspNetCore.Filters
{
    /// <summary>
    /// Adds Last-Modified and ETag headers when result is <see cref="ObjectResult"/>
    /// and model implements <see cref="IETaggable"/> or <see cref="ITimestamped"/>.
    /// Also, shortcuts the response with 304 Not Modified when request/response headers match.
    /// </summary>
    public class EntityHttpCacheResponseHeadersAttribute : ActionFilterAttribute
    {
        /// <inheritdoc/>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Result is ObjectResult objectResult)
            {
                if (objectResult.Value is ITimestamped timestamped && timestamped.Timestamp.HasValue)
                {
                    context.HttpContext.Response.GetTypedHeaders().LastModified = timestamped.Timestamp.Value;
                    if (context.HttpContext.Request.GetTypedHeaders().IfModifiedSince != null
                     && context.HttpContext.Request.GetTypedHeaders().IfModifiedSince >= context.HttpContext.Response.GetTypedHeaders().LastModified)
                    {
                        context.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
                    }
                }

                if (objectResult.Value is IETaggable taggable && !string.IsNullOrEmpty(taggable.ETag))
                {
                    if (EntityTagHeaderValue.TryParse(taggable.ETag, out var parsedEtag))
                    {
                        context.HttpContext.Response.GetTypedHeaders().ETag = parsedEtag;
                    }
                    else
                    {
                        context.HttpContext.Response.GetTypedHeaders().ETag = new EntityTagHeaderValue($"\"{taggable.ETag}\"", true);
                    }

                    if (context.HttpContext.Request.GetTypedHeaders().IfNoneMatch != null
                     && context.HttpContext.Request.GetTypedHeaders().IfNoneMatch.Any()
                     && context.HttpContext.Request.GetTypedHeaders().IfNoneMatch.First().Compare(context.HttpContext.Response.GetTypedHeaders().ETag, !context.HttpContext.Response.GetTypedHeaders().ETag.IsWeak))
                    {
                        context.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
                    }
                }
            }
        }
    }
}
