using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Pantry.AspNetCore.Filters
{
    /// <summary>
    /// Adds Last-Modified and ETag headers when result is <see cref="ObjectResult"/>
    /// and model implements <see cref="IETaggable"/> or <see cref="ITimestamped"/>.
    /// </summary>
    public class EntityResponseHeadersAttribute : ActionFilterAttribute
    {
        /// <inheritdoc/>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            if (context.Result is ObjectResult objectResult)
            {
                if (context.HttpContext.Response.GetTypedHeaders().LastModified is null
                    && objectResult.Value is ITimestamped timestamped
                    && timestamped.Timestamp.HasValue)
                {
                    context.HttpContext.Response.GetTypedHeaders().LastModified = timestamped.Timestamp;
                }

                if (context.HttpContext.Response.GetTypedHeaders().ETag is null
                    && objectResult.Value is IETaggable taggable
                    && !string.IsNullOrEmpty(taggable.ETag))
                {
                    context.HttpContext.Response.GetTypedHeaders().ETag = new EntityTagHeaderValue($"\"{taggable.ETag}\"", true);
                }
            }
        }
    }
}
