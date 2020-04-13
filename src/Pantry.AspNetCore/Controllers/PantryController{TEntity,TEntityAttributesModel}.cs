using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Omu.ValueInjecter;
using Pantry.AspNetCore.Filters;
using Pantry.Mapping;
using Pantry.Traits;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Base class for REST controllers.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityAttributesModel">The model that contains the attributes.</typeparam>
    [ApiController]
    [EntityResponseHeaders]
    public abstract class PantryController<TEntity, TEntityAttributesModel> : ControllerBase
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PantryController{TEntity, TEntityAttributesModel}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public PantryController(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Implements the Create action.
        /// </summary>
        /// <param name="model">The model attributes.</param>
        /// <returns>The created entity.</returns>
        protected virtual async Task<TEntity> CreateAction(TEntityAttributesModel model)
        {
            var repository = ServiceProvider.GetRequiredService<IRepositoryAdd<TEntity>>();
            var mapper = ServiceProvider.GetService<IMapper<TEntityAttributesModel, TEntity>>();

            TEntity entity;
            if (mapper != null)
            {
                entity = mapper.MapToDestination(model);
            }
            else
            {
                entity = Mapper.Map<TEntityAttributesModel, TEntity>(model);
            }

            return await repository.AddAsync(entity).ConfigureAwait(false);
        }

        /// <summary>
        /// Implements the Get action.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>The entity, or null if not found.</returns>
        protected virtual async Task<TEntity?> GetAction(string id)
        {
            var repository = ServiceProvider.GetRequiredService<IRepositoryGet<TEntity>>();

            return await repository.TryGetByIdAsync(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Return either 200 OK, 404 NotFound or 304 Not Modified depending on the <paramref name="result"/> and the current Request headers.
        /// If <paramref name="result"/> is null, return not found.
        /// If <paramref name="result"/> is not null and does not implement <see cref="IETaggable"/> or <see cref="ITimestamped"/>, returns OK.
        /// If <paramref name="result"/> is not null and implements <see cref="IETaggable"/> or <see cref="ITimestamped"/>, returns either OK or Not Modified depending on the request headers.
        /// </summary>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="result">The result.</param>
        /// <returns>The result based on the rules outlined above.</returns>
        protected virtual ActionResult<TResult> OkNotFoundOrNotModified<TResult>(TResult? result)
            where TResult : class
        {
            if (result is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"{HttpContext.Request.Path} cannot be found.");
            }
            else
            {
                ActionResult<TResult>? actionResult = null;
                if (result is ITimestamped timestamped && timestamped.Timestamp.HasValue)
                {
                    Response.GetTypedHeaders().LastModified = timestamped.Timestamp.Value;
                    if (Request.GetTypedHeaders().IfModifiedSince != null
                     && Request.GetTypedHeaders().IfModifiedSince >= Response.GetTypedHeaders().LastModified)
                    {
                        actionResult = StatusCode(StatusCodes.Status304NotModified);
                    }
                }

                if (result is IETaggable taggable && !string.IsNullOrEmpty(taggable.ETag))
                {
                    Response.GetTypedHeaders().ETag = new EntityTagHeaderValue($"\"{taggable.ETag}\"", true);

                    if (Request.GetTypedHeaders().IfNoneMatch != null
                     && Request.GetTypedHeaders().IfNoneMatch.Any()
                     && Request.GetTypedHeaders().IfNoneMatch.First().Compare(Response.GetTypedHeaders().ETag, false))
                    {
                        actionResult = StatusCode(StatusCodes.Status304NotModified);
                    }
                }

                return actionResult ?? Ok(result);
            }
        }
    }
}
